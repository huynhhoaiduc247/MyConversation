namespace MyConversation.SignalR
{
    public class ConnectionManagement
    {
        public static IDictionary<string, List<string>> Connections = new Dictionary<string, List<string>>();
        
        public static void Add(string Username, string ConnectionId)
        {
            try
            {
                if (Connections.ContainsKey(Username))
                {
                    Connections[Username].Clear();
                    Connections[Username].Add(ConnectionId);
                }
                else
                {
                    Connections.Add(Username, new List<string>() { ConnectionId });
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static void Remove(string Username, string ConnectionId)
        {
            try
            {
                if (Connections.ContainsKey(Username))
                {
                    Connections[Username].Remove(ConnectionId);
                    if (Connections[Username].Count == 0)
                    {
                        Connections.Remove(Username);
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
