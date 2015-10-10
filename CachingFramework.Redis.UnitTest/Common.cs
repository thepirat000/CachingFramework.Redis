using CachingFramework.Redis.Serializers;

namespace CachingFramework.Redis.UnitTest
{
    internal static class Common
    {
        public static Context GetContextAndFlush()
        {
            // Config doc: https://github.com/StackExchange/StackExchange.Redis/blob/master/Docs/Configuration.md
            var config =
                "192.168.15.15:7001,192.168.15.15:7006,192.168.15.15:7002,192.168.15.15:7003,192.168.15.15:7004,192.168.15.15:7005,192.168.15.15:7000,connectRetry=10,syncTimeout=5000,abortConnect=false,keepAlive=10, allowAdmin=true";
            config = "192.168.15.11:6379, allowAdmin=true";
            var ctx = new Context(config, new BinarySerializer());
            ctx.Cache.FlushAll();
            return ctx;
        }
    }
}
