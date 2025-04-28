using Microsoft.Extensions.Configuration;

namespace T3H.Poll.CrossCuttingConcerns.Helper
{
    public static class ConnectionHelper
    {
        private static IConfiguration configuration { get; set; }
        private static string _redisConn;
        private static string _redisDbIndex;

        public static void InitSetting()
        {
            var builder = new ConfigurationBuilder()
                       .SetBasePath(Directory.GetCurrentDirectory())
                         .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                         .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}.json", true)
                         .AddEnvironmentVariables();
            configuration = builder.Build();
        }

        public static string RedisConn
        {
            get
            {
                if (string.IsNullOrEmpty(_redisConn))
                {
                    InitSetting();
                    _redisConn = configuration.GetValue("RedisConfig:RedisConn", "");
                }
                return _redisConn;
            }
        }

        public static string RedisDbIndex
        {
            get
            {
                if (string.IsNullOrEmpty(_redisDbIndex))
                {
                    InitSetting();
                    _redisDbIndex = configuration.GetValue("RedisConfig:DatabaseIndex", "0");
                }
                return _redisDbIndex;
            }
        }

    }
}
