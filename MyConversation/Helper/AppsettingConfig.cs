namespace MyConversation.Helper
{
    public static class AppsettingConfig
    {
        public static string GetByKey(string key)
        {
            try
            {
                var configuration = new ConfigurationBuilder().AddJsonFile($"appsettings.json");
                var config = configuration.Build();
                return config.GetSection(key).Value.ToString();
            }
            catch(Exception ex)
            {
                return string.Empty;
            }
        }
    }
}
