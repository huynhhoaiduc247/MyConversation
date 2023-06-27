using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using MyConversation.Helper;
using MyConversation.Model.Common;
using MyConversation.Model.Model;
using MyConversation.Model.ModelParam;
using MyConversation.Model.SystemModel;
using MyConversation.Repository.Helper;
using MyConversation.Repository.Repository;
using MyConversation.SignalR;

namespace MyConversation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ConversationController : ControllerBase
    {
        private readonly Microsoft.Extensions.Logging.ILogger _logger;
        private readonly IHubContext<ConversationHub> _hubcontext;

        public ConversationController(Microsoft.Extensions.Logging.ILogger<Conversation> logger, IHubContext<ConversationHub> hubcontext)
        {
            _logger = logger;
            _hubcontext = hubcontext;
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
                    var response = conversationRep.All(page, pageSize, x => x.Active == true && x.Attendance.Any(x => x == user.Username));
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

        [Route("detail")]
        [HttpGet]
        public ActionResult Detail(string Id)
        {
            try
            {
                var user = CommonHandler.HandleToken(Request.Headers["Authorization"].ToString());
                using (var conversationRep = new ConversationRepository(user.ClientId))
                {
                    var response = conversationRep.Single(x => x.Id == Id && x.Attendance.Any(x => x == user.Username) && x.Active == true);
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

        [Route("create-private")]
        [HttpPost]
        public ActionResult CreatePrivate([FromBody] Conversation conversation)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest("Model error");
                }
                var user = CommonHandler.HandleToken(Request.Headers["Authorization"].ToString());
                conversation.Attendance.Add(user.Username);
                conversation.Attendance = conversation.Attendance.Distinct().ToList();
                string Id = string.Empty;
                using (var conversationRep = new ConversationRepository(user.ClientId))
                {
                    Response<Conversation> response = new Response<Conversation>();
                    if (conversation.ConversationType == EnumDefinition.ConversationType.normal)
                    {
                        if (conversation.Attendance.Count > 2)
                        {
                            return BadRequest("This conversation type limit attendance on 2 member");
                        }
                        var existed = conversationRep.Single(x => x.ConversationType == EnumDefinition.ConversationType.normal
                            && conversation.Attendance.All(y => x.Attendance.Any(z => z == y)
                        ));
                        if (existed.Data != null)
                        {
                            return Ok(existed);
                        }
                        var item = new Conversation();
                        Id = item.Id;
                        item.Active = true;
                        item.Name = conversation.Name;
                        item.ClientId = user.ClientId;
                        item.StartBy = user.Username;
                        item.StartDate = DateTime.Now;
                        item.ConversationType = conversation.ConversationType;
                        item.CreatedBy = user.Username;
                        item.ModifiedBy = user.Username;
                        item.Attendance = conversation.Attendance;
                        item.UserRead = item.Attendance;
                        response = conversationRep.Add(item);
                    }
                    else
                    {
                        var item = new Conversation();
                        Id = item.Id;
                        item.Active = true;
                        item.Name = conversation.Name;
                        item.ClientId = user.ClientId;
                        item.StartBy = user.Username;
                        item.StartDate = DateTime.Now;
                        item.ConversationType = conversation.ConversationType;
                        item.CreatedBy = user.Username;
                        item.ModifiedBy = user.Username;
                        item.Attendance = conversation.Attendance;
                        item.UserRead = item.Attendance;
                        response = conversationRep.Add(item);
                    }

                    var createdItem = conversationRep.Single(x => x.Id == Id);
                    return Ok(createdItem);
                }
            }
            catch (Exception ex)
            {
                return Problem(ex.Message);
            }
        }

        //[Route("update")]
        //[HttpPost]
        //public ActionResult Update([FromBody] ClientApp clientApp)
        //{
        //    try
        //    {
        //        if (!ModelState.IsValid)
        //        {
        //            return BadRequest("Model error");
        //        }
        //        var user = CommonHandler.HandleToken(Request.Headers["Authorization"].ToString());
        //        if (!user.IsRoot)
        //        {
        //            return BadRequest("This user not allow to do this");
        //        }
        //        using (var clientAppRep = new ClientAppRepository(CommonHandler.GeneralDB))
        //        {
        //            var existedClientApp = clientAppRep.Single(x => x.Id == clientApp.Id);
        //            if (existedClientApp.Data == null)
        //            {
        //                return BadRequest("Can't find this item");
        //            }

        //            clientApp.ClientId = existedClientApp.Data.ClientId;
        //            clientApp.UserAdmin = existedClientApp.Data.UserAdmin;
        //            clientApp.InitPassword = existedClientApp.Data.InitPassword;

        //            clientApp = (ClientApp)CommonHandler.UpdateField(clientApp, user.Username);
        //            var response = clientAppRep.UpdateOne(clientApp);
        //            if (!response.IsSuccess)
        //            {
        //                throw new Exception(response.Message);
        //            }
        //            return Ok(response);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        return Problem(ex.Message);
        //    }
        //}

        #endregion

        #region message detail
        [Route("send")]
        [HttpPost]
        public ActionResult Send(SendModel sendModel)
        {
            try
            {
                var user = CommonHandler.HandleToken(Request.Headers["Authorization"].ToString());
                using (var messageRep = new MessageDetailRepository(user.ClientId))
                {
                    using (var conversationRep = new ConversationRepository(user.ClientId)) 
                    {
                        var responseConversation = conversationRep.Single(x => x.Active && x.Id == sendModel.ConversationId && x.Attendance.Any(y => y == user.Username));
                        if (responseConversation.Data == null)
                        {
                            return BadRequest("This user is not in this conversation");
                        }

                        var To = responseConversation.Data.Attendance.Except(new List<string> { user.Username }).ToList();
                        var response = messageRep.Add(new MessageDetail()
                        {
                            Active = true,
                            ClientId = user.ClientId,
                            Content = sendModel.Message,
                            ConversationId = sendModel.ConversationId,
                            From = user.Username,
                            To = To,
                            UserRead = new List<string>() { user.Username },
                            CreatedBy = user.Username,
                        });

                        responseConversation.Data.LastActionBy = user.Username;
                        responseConversation.Data.LastActionDate = DateTime.Now;
                        responseConversation.Data.LastMessage = sendModel.Message;
                        responseConversation.Data.UserRead = new List<string>() { user.Username };
                        conversationRep.UpdateOne(responseConversation.Data, "LastActionBy;LastActionDate;LastMessage;UserRead");

                        ConversationClientHub.SendMessage(_hubcontext, To, sendModel.Message, user.Username, sendModel.ConversationId);
                        return Ok(response);
                    }
                }
            }
            catch (Exception ex)
            {
                return Problem(ex.Message);
            }
        }

        [Route("message-detail")]
        [HttpGet]
        public ActionResult MessageDetail(string ConversationId, int page = 1, int pageSize = 10)
        {
            try
            {
                var user = CommonHandler.HandleToken(Request.Headers["Authorization"].ToString());
                using (var conversationRep = new ConversationRepository(user.ClientId))
                {
                    var response = conversationRep.Single(x => x.Active && x.Id == ConversationId && x.Attendance.Any(y => y == user.Username));
                    if (response.Data == null)
                    {
                        return BadRequest("This user is not in this conversation");
                    }
                }
                using (var messageRep = new MessageDetailRepository(user.ClientId))
                {
                    var response = messageRep.All(page, pageSize, x => x.Active == true && x.ConversationId == ConversationId,
                        new Sort<MessageDetail>()
                        {
                            IsAscent = false,
                            expression = x => x.CreatedDate
                        });
                    if (!response.IsSuccess)
                    {
                        throw new Exception(response.Message);
                    }
                    response.Data = response.Data.OrderBy(x => x.CreatedDate);
                    return Ok(response);
                }
            }
            catch (Exception ex)
            {
                return Problem(ex.Message);
            }
        }



        [Route("mark-read")]
        [HttpPost]
        public ActionResult MarkRead([FromQuery]string ConversationId)
        {
            try
            {
                var user = CommonHandler.HandleToken(Request.Headers["Authorization"].ToString());
                using (var conversationRep = new ConversationRepository(user.ClientId))
                {
                    var responseConversation = conversationRep.Single(x => x.Active && x.Id == ConversationId && x.Attendance.Any(y => y == user.Username));
                    if (responseConversation.Data == null)
                    {
                        return BadRequest("This user is not in this conversation");
                    }
                    using (var messageRep = new MessageDetailRepository(user.ClientId))
                    { 
                        var messDetailCol = DbContext.Instance.GetDB(user.ClientId).GetCollection<MessageDetail>("MessageDetail");
                        messDetailCol.UpdateMany(MongoDB.Driver.Builders<MessageDetail>.Filter.Eq("Active", true)
                            & MongoDB.Driver.Builders<MessageDetail>.Filter.Eq("ConversationId", ConversationId)
                            & MongoDB.Driver.Builders<MessageDetail>.Filter.Nin("UserRead", new List<string> { user.Username }),
                            MongoDB.Driver.Builders<MessageDetail>.Update.Push("UserRead", user.Username));

                        var conversationCol = DbContext.Instance.GetDB(user.ClientId).GetCollection<Conversation>("Conversation");
                        conversationCol.UpdateOne(MongoDB.Driver.Builders<Conversation>.Filter.Eq("Active", true)
                            & MongoDB.Driver.Builders<Conversation>.Filter.Eq("_id", ConversationId)
                            & MongoDB.Driver.Builders<Conversation>.Filter.Nin("UserRead", new List<string> { user.Username }),
                            MongoDB.Driver.Builders<Conversation>.Update.Push("UserRead", user.Username));
                        return Ok();
                    }
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
