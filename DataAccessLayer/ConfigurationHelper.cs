using Microsoft.Extensions.Configuration;


namespace DataAccessLayer
{
        static class ConfigurationHelper
        {
            private static IConfiguration? _configuration;

            public static IConfiguration GetConfiguration()
            {
                if (_configuration == null)
                {
                    _configuration = new ConfigurationBuilder()
                        .SetBasePath(Directory.GetCurrentDirectory())
                        .AddJsonFile("appsettings.json")
                        .Build();
                }
                return _configuration;
            }
        }
    
}
