using Microsoft.AspNetCore.SignalR;
using MyConversation.Helper;
using MyConversation.Model.Model;
using MyConversation.Repository.Repository;

namespace MyConversation.SignalR
{
    public class ConversationHub: Hub
    {
        #region handle connect
        public override Task OnConnectedAsync()
        {
            try
            {
                var a = Context.GetHttpContext()?.Request;
                var user = Context.GetHttpContext()?.Request.Query["user"].ToString();
                var connectionId = Context.ConnectionId;
                if (!string.IsNullOrEmpty(user) && !string.IsNullOrEmpty(connectionId))
                {
                    ConnectionManagement.Add(user, connectionId);
                    Clients.AllExcept(connectionId).SendAsync("Online", user);

                    //update status user
                    using (var repUser = new UserRepository(CommonHandler.GeneralDB))
                    {
                        var existedUser = repUser.Single(x => x.Username == user);
                        if (existedUser.Data != null)
                        {
                            existedUser.Data = (User)CommonHandler.UpdateField(existedUser.Data);
                            existedUser.Data.Status = Model.Common.EnumDefinition.UserStatus.online;
                            repUser.UpdateOne(x => x.Username == user, existedUser.Data, "Status;CurrentToken;ModifiedDate;ModifiedBy");
                        }
                    }
                }
            }
            catch (Exception ex)
            {

            }
            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception? exception=null)
        {
            try
            {
                var user = Context.GetHttpContext()?.Request.Query["user"].ToString();
                var connectionId = Context.ConnectionId;
                if (!string.IsNullOrEmpty(user) && !string.IsNullOrEmpty(connectionId))
                {
                    ConnectionManagement.Remove(user, connectionId);
                    Clients.AllExcept(connectionId).SendAsync("Offline", user);

                    //update status user
                    using (var repUser = new UserRepository(CommonHandler.GeneralDB))
                    {
                        var existedUser = repUser.Single(x => x.Username == user);
                        if (existedUser.Data != null)
                        {
                            existedUser.Data.CurrentToken = null;
                            existedUser.Data = (User)CommonHandler.UpdateField(existedUser.Data);
                            existedUser.Data.Status = Model.Common.EnumDefinition.UserStatus.offline;
                            repUser.UpdateOne(x => x.Username == user, existedUser.Data, "Status;CurrentToken;ModifiedDate;ModifiedBy");
                        }
                    }
                }
            }
            catch (Exception ex)
            {

            }
            return base.OnDisconnectedAsync(exception);
        }
        #endregion

        #region handle
        public async Task SendMessage(List<string> user, string message)
        {
            if (user.Count == 0)
            {
                return;
            }
            var connectionIds = user
                .Select(x => ConnectionManagement.Connections.FirstOrDefault(y => y.Key == x).Value?.FirstOrDefault() ?? string.Empty)
                .Except(new List<string> { Context.ConnectionId }).Where(x => !string.IsNullOrEmpty(x)).ToList();
            await Clients.Clients(connectionIds).SendAsync("ReceiveMessage", user, message);
        }
        #endregion
    }
}
