namespace ToDoListBlazor
{
    public static class Configuration
    {
        private static string _currentProjectUrl;
        public static string CurrentProjectUrl
        {
            get
            {
                return _currentProjectUrl;
            }
            set
            {
                _currentProjectUrl = value;
            }
        }

        private static string _webApiUrl;
        public static string WebApiUrl
        {
            get
            {
                return _webApiUrl;
            }
            set
            {
                _webApiUrl = value;
            }
        }


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
