using JWT.Algorithms;
using JWT.Builder;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MyConversation.Helper;
using MyConversation.Model.Model;
using MyConversation.Model.ModelParam;
using MyConversation.Model.SystemModel;
using MyConversation.Repository.Repository;

namespace MyConversation.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AccountController : ControllerBase
    {
        private readonly Microsoft.Extensions.Logging.ILogger _logger;

        public AccountController(Microsoft.Extensions.Logging.ILogger<User> logger)
        {
            _logger = logger;
        }
        /// <summary>
        /// Test api to check service is alive
        /// </summary>
        /// <returns></returns>
        [Route("test")]
        [HttpGet]
        public ActionResult Test()
        {
            return Ok();
        }

        [Route("login")]
        [HttpPost]
        public ActionResult Login(LoginModel userLogin)
        {
            try
            {
                using (var userRep = new UserRepository(CommonHandler.GeneralDB))
                {
                    var userResponse = userRep.Single(x => x.Username == userLogin.Username && x.Password == userLogin.Password && x.Active);
                    if (userResponse.IsSuccess)
                    {
                        var user = userResponse.Data;
                        if (user == null)
                        {
                            return BadRequest("User not found");
                        }
                        var SecretKeyJWT = AppsettingConfig.GetByKey("SecretKeyJWT");
                        var token = JwtBuilder.Create()
                                  .WithAlgorithm(new HMACSHA512Algorithm()) // symmetric
                                  .WithSecret(SecretKeyJWT)
                                  .AddClaim("exp", DateTimeOffset.UtcNow.AddHours(16).ToUnixTimeSeconds())
                                  .AddClaim("username", user.Username)
                                  .AddClaim("password", user.Password)
                                  .Encode();

                        user.Status = Model.Common.EnumDefinition.UserStatus.online;
                        user = (User)CommonHandler.UpdateField(user);
                        user.CurrentToken = token;
                        userRep.UpdateOne(user, "Status;CurrentToken;ModifiedDate;ModifiedBy");
                        return Ok(new Response<string>() { 
                            IsSuccess = true,
                            Data = token
                        });
                    }
                    else
                    {
                        throw new Exception(userResponse.Message);
                    }
                }
            }
            catch (Exception ex)
            {
                return Problem(ex.Message);
            }
        }

        [Route("logout")]
        [HttpGet]
        public ActionResult Logout()
        {
            try
            {
                using (var userRep = new UserRepository(CommonHandler.GeneralDB))
                {
                    var user = CommonHandler.HandleToken(Request.Headers["Authorization"].ToString());
                    if (user == null)
                    {
                        return BadRequest("User not found");
                    }
                    user.Status = Model.Common.EnumDefinition.UserStatus.offline;
                    user = (User)CommonHandler.UpdateField(user);
                    user.CurrentToken = null;
                    userRep.UpdateOne(user, "Status;CurrentToken;ModifiedDate;ModifiedBy");

                    return Ok(new Response<string>()
                    {
                        IsSuccess = true
                    });
                }
            }
            catch (Exception ex)
            {
                return Problem(ex.Message);
            }
        }



        #region CRUD
        [Route("index")]
        [HttpGet]
        public ActionResult Index(int page = 1, int pageSize = 10)
        {
            try
            {
                var user = CommonHandler.HandleToken(Request.Headers["Authorization"].ToString());
                using (var conversationRep = new ConversationRepository(user.ClientId)) 
                {
                    var responseConversation = conversationRep.All(null, null, x => x.Active == true && x.ClientId == user.ClientId
                        && x.Attendance.Contains(user.Username));
                    var groupConversation = responseConversation.Data?.Where(x => x.ConversationType == Model.Common.EnumDefinition.ConversationType.group).ToList();

                    using (var userRep = new UserRepository(CommonHandler.GeneralDB))
                    {
                        var responseUser = userRep.All(null, null, x => x.Active == true && x.ClientId == user.ClientId);
                        Response<List<UserCustomModel>> userCustomModel = new Response<List<UserCustomModel>>();
                        userCustomModel.IsSuccess = responseUser.IsSuccess;
                        userCustomModel.Status = responseUser.Status;
                        userCustomModel.Message = responseUser.Message;

                        userCustomModel.Data = new List<UserCustomModel>();
                        if (groupConversation != null) 
                        {
                            foreach (var item in groupConversation)
                            {
                                userCustomModel.Data.Add(new UserCustomModel()
                                {
                                    Conversation = item,
                                    ConversationId = item.Id
                                });
                            }
                        }
                        userCustomModel.Data.AddRange(responseUser.Data.Select(x => new UserCustomModel() { 
                            Active = x.Active,
                            Address = x.Address,
                            ClientId = x.ClientId,
                            CreatedBy = x.CreatedBy,
                            CreatedDate = x.CreatedDate,
                            CurrentToken = x.CurrentToken,
                            DOB = x.DOB,
                            Id = x.Id,
                            IsAdmin = x.IsAdmin,
                            IsRoot = x.IsRoot,
                            ModifiedBy = x.ModifiedBy,
                            ModifiedDate = x.ModifiedDate,
                            Name = x.Name,
                            Password = String.Empty,
                            Phone = x.Phone,
                            Status = x.Status,
                            Username = x.Username,
                            ConversationId = responseConversation.Data?.FirstOrDefault(y => y.Attendance.Count == 2 
                                && y.Attendance.All(z => new List<string> { x.Username, user.Username}.Contains(z)))?.Id ?? String.Empty,
                            Conversation = responseConversation.Data?.FirstOrDefault(y => y.Attendance.Count == 2
                                && y.Attendance.All(z => new List<string> { x.Username, user.Username }.Contains(z)))
                        }));

                        if (!userCustomModel.IsSuccess)
                        {
                            throw new Exception(userCustomModel.Message);
                        }

                        return Ok(userCustomModel);
                    }
                }
            }
            catch (Exception ex)
            {
                return Problem(ex.Message);
            }
        }

        [Route("detail")]
        [HttpGet]
        public ActionResult Detail(int page = 1, int pageSize = 10)
        {
            try
            {
                var user = CommonHandler.HandleToken(Request.Headers["Authorization"].ToString());
                using (var userRep = new UserRepository(CommonHandler.GeneralDB))
                {
                    var response = userRep.All(page, pageSize, x => x.Active == true && x.ClientId == user.ClientId);
                    if (!response.IsSuccess)
                    {
                        throw new Exception(response.Message);
                    }

                    return Ok(response);
                }
            }
            catch (Exception ex)
            {
                return Problem(ex.Message);
            }
        }

        [Route("create")]
        [HttpPost]
        public ActionResult Create([FromBody] User item)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest("Model error");
                }
                var user = CommonHandler.HandleToken(Request.Headers["Authorization"].ToString());
                if (!user.IsAdmin && !user.IsRoot)
                {
                    return BadRequest("This user not allow to do this");
                }
                if (item.IsRoot && !user.IsRoot)
                {
                    return BadRequest("This user not allow to create root user");
                }
                using (var userRep = new UserRepository(CommonHandler.GeneralDB))
                {
                    var existed = userRep.Single(x => x.Username == item.Username);
                    if (existed.Data != null)
                    {
                        return BadRequest("This Username has been created before");
                    }

                    item.Id = ObjectId.GenerateNewId().ToString();
                    item.CreatedBy = user.Username;
                    item.CreatedDate = DateTime.Now;
                    item.ClientId = user.ClientId;
                    var response = userRep.Add(item);
                    if (!response.IsSuccess)
                    {
                        throw new Exception(response.Message);
                    }
                    return Ok(response);
                }
            }
            catch (Exception ex)
            {
                return Problem(ex.Message);
            }
        }

        [Route("update")]
        [HttpPost]
        public ActionResult Update([FromBody] User item)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest("Model error");
                }
                var user = CommonHandler.HandleToken(Request.Headers["Authorization"].ToString());
                if (!user.IsRoot)
                {
                    return BadRequest("This user not allow to do this");
                }
                using (var userRep = new UserRepository(CommonHandler.GeneralDB))
                {
                    var existed = userRep.Single(x => x.Username == item.Username && x.Id != item.Id);
                    if (existed.Data != null)
                    {
                        return BadRequest("This Username has been created before");
                    }

                    var existedUser = userRep.Single(x => x.Id == item.Id);
                    if (existedUser.Data == null)
                    {
                        return BadRequest("Can't find this item");
                    }

                    item.ClientId = existedUser.Data.ClientId;
                    item.Username = existedUser.Data.Username;

                    item = (User)CommonHandler.UpdateField(item, user.Username);
                    var response = userRep.UpdateOne(item, "Name;Phone;Address;DOB;IsAdmin;IsRoot");
                    if (!response.IsSuccess)
                    {
                        throw new Exception(response.Message);
                    }
                    return Ok(response);
                }
            }
            catch (Exception ex)
            {
                return Problem(ex.Message);
            }
        }


        [Route("in-active")]
        [HttpPost]
        public ActionResult InActive([FromBody] User item)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest("Model error");
                }
                var user = CommonHandler.HandleToken(Request.Headers["Authorization"].ToString());
                if (!user.IsRoot)
                {
                    return BadRequest("This user not allow to do this");
                }
                using (var userRep = new UserRepository(CommonHandler.GeneralDB))
                {
                    item = (User)CommonHandler.UpdateField(item, user.Username);
                    item.Active = false;
                    var response = userRep.UpdateOne(item, "Active;ModifiedDate;ModifiedBy");
                    if (!response.IsSuccess)
                    {
                        throw new Exception(response.Message);
                    }
                    return Ok(response);
                }
            }
            catch (Exception ex)
            {
                return Problem(ex.Message);
            }
        }

        [Route("delete")]
        [HttpPost]
        public ActionResult Delete([FromBody] User item)
        {
            try
            {
                //if (!ModelState.IsValid)
                //{
                //    return BadRequest("Model error");
                //}
                using (var userRep = new UserRepository(CommonHandler.GeneralDB))
                {
                    var response = userRep.Delete(x => x.Id == item.Id);
                    if (!response.IsSuccess)
                    {
                        throw new Exception(response.Message);
                    }
                    return Ok(response);
                }
            }
            catch (Exception ex)
            {
                return Problem(ex.Message);
            }
        }
        #endregion
    }
}
