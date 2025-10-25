namespace ToDoListApi
{
    public static class Configuration
    {
        private static IConfiguration? _config;
        public static IConfiguration AppSettings
        {
            get
            {
                if (_config != null) return _config;

#if DEBUG
                var builder = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.Development.json");
#else
                var builder = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json");
#endif

                _config = builder.Build();
                return _config;
            }
        }
    }
}
