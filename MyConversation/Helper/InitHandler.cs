using MyConversation.Repository.Helper;

namespace MyConversation.Helper
{
    public class InitHandler
    {
        public static void Init()
        {
            var connectionString = AppsettingConfig.GetByKey("ConnectionString");
            DbContext.Instance.Init(connectionString);
        }
    }
}
