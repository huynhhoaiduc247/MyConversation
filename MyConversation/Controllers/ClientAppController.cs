using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MyConversation.Helper;
using MyConversation.Model.Model;
using MyConversation.Repository.Repository;

namespace MyConversation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ClientAppController : ControllerBase
    {
        private readonly Microsoft.Extensions.Logging.ILogger _logger;

        public ClientAppController(Microsoft.Extensions.Logging.ILogger<ClientApp> logger)
        {
            _logger = logger;
        }

        #region CRUD
        [Route("index")]
        [HttpGet]
        public ActionResult Index(int page = 1, int pageSize = 10)
        {
            try
            {
                var user = CommonHandler.HandleToken(Request.Headers["Authorization"].ToString());
                if (!user.IsRoot)
                {
                    return BadRequest("This user not allow to do this");
                }
                using (var clientAppRep = new ClientAppRepository(CommonHandler.GeneralDB))
                {
                    var response = clientAppRep.All(page, pageSize, x => x.Active == true);
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
        public ActionResult Create([FromBody] ClientApp clientApp)
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
                using (var clientAppRep = new ClientAppRepository(CommonHandler.GeneralDB))
                {
                    using (var userRep = new UserRepository(CommonHandler.GeneralDB)) {
                        var existedUser = userRep.Single(x => x.Username == clientApp.UserAdmin);
                        if (existedUser.Data != null)
                        {
                            return BadRequest("This admin user is existed");
                        }
                        clientApp.Id = ObjectId.GenerateNewId().ToString();
                        clientApp.ClientId = CommonHandler.GenerateID(clientApp.Name);
                        clientApp.CreatedBy = user.Username;
                        clientApp.CreatedDate = DateTime.Now;
                        var response = clientAppRep.Add(clientApp);
                        if (!response.IsSuccess)
                        {
                            throw new Exception(response.Message);
                        }
                        userRep.Add(new User()
                        {
                            Active = true,
                            IsAdmin = true,
                            ClientId = clientApp.ClientId,
                            Username = clientApp.UserAdmin,
                            Password = clientApp.InitPassword,
                            CreatedBy = user.Username,
                            Name = user.Username,
                        });
                        return Ok(response);
                    }
                }
            }
            catch (Exception ex)
            {
                return Problem(ex.Message);
            }
        }
        
        [Route("update")]
        [HttpPost]
        public ActionResult Update([FromBody] ClientApp clientApp)
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
                using (var clientAppRep = new ClientAppRepository(CommonHandler.GeneralDB))
                {
                    var existedClientApp = clientAppRep.Single(x => x.Id == clientApp.Id);
                    if (existedClientApp.Data == null)
                    {
                        return BadRequest("Can't find this item");
                    }

                    clientApp.ClientId = existedClientApp.Data.ClientId;
                    clientApp.UserAdmin = existedClientApp.Data.UserAdmin;
                    clientApp.InitPassword = existedClientApp.Data.InitPassword;

                    clientApp = (ClientApp)CommonHandler.UpdateField(clientApp, user.Username);
                    var response = clientAppRep.UpdateOne(clientApp);
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
        public ActionResult InActive([FromBody] ClientApp clientApp)
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
                using (var clientAppRep = new ClientAppRepository(CommonHandler.GeneralDB))
                {
                    clientApp = (ClientApp)CommonHandler.UpdateField(clientApp, user.Username);
                    clientApp.Active = false;
                    var response = clientAppRep.UpdateOne(clientApp, "Active;ModifiedDate;ModifiedBy");
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
        public ActionResult Delete([FromBody] ClientApp clientApp)
        {
            try
            {
                //if (!ModelState.IsValid)
                //{
                //    return BadRequest("Model error");
                //}
                using (var clientAppRep = new ClientAppRepository(CommonHandler.GeneralDB))
                {
                    var response = clientAppRep.Delete(x=>x.Id == clientApp.Id);
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
