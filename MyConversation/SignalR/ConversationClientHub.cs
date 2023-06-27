using Microsoft.AspNetCore.SignalR;

namespace MyConversation.SignalR
{
    public class ConversationClientHub
    {
        public static void SendMessage(IHubContext<ConversationHub> hubContext, List<string> user, string message, string currentUser, string conversationId)
        {
            if (user.Count == 0)
            {
                return;
            }
            var connectionIds = user
                .Except(new List<string>() { currentUser })
                .Select(x => ConnectionManagement.Connections.FirstOrDefault(y => y.Key == x).Value?.FirstOrDefault() ?? string.Empty)
                .Where(x => !string.IsNullOrEmpty(x)).ToList();

            object response = new { message = message, conversationId = conversationId };
            hubContext.Clients.Clients(connectionIds).SendAsync("ReceiveMessage", currentUser, response);
        }
    }
}
