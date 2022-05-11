using System;
using System.Linq;
using System.Threading;
using CachingFramework.Redis.Serializers;

namespace CachingFramework.Redis.UnitTest
{
    public static class Common
    {
        // A context using a raw serializer
        private static RedisContext _rawContext;
        // A context using a binary serializer
        private static RedisContext _binaryContext;
        // A context using json
        private static RedisContext _jsonContext;
        // A context using newtonsoft json
        private static RedisContext _newtonsoftJsonContext;
        // A context using msgpack
        private static RedisContext _msgPackContext;


        // TestCases
        public static RedisContext[] JsonAndRaw { get { return new[] { _jsonContext, _rawContext, _newtonsoftJsonContext }; } }
        public static RedisContext[] Json { get { return new[] { _jsonContext, _newtonsoftJsonContext }; } }
        public static RedisContext[] NewtonsoftJson { get { return new[] { _newtonsoftJsonContext }; } }
        public static RedisContext[] MsgPack { get { return new[] { _msgPackContext }; } }
        public static RedisContext[] Raw { get { return new[] { _rawContext }; } }
        public static RedisContext[] Bin { get { return new[] { _binaryContext }; } }
        public static RedisContext[] All { get; set; }
        public static RedisContext[] BinAndRawAndJson { get; set; }

        public static DateTime ServerNow
        {
            get
            {
                var cnn = _rawContext.GetConnectionMultiplexer();
                var serverNow = cnn.GetServer(cnn.GetEndPoints()[0]).Time();
                return serverNow;
            }
        }

        public static int[] VersionInfo { get; set; }
        public static string Config = "localhost:6379, allowAdmin=true"; 

        static Common()
        {
            
            _rawContext = new RedisContext(Config, new RawSerializer());
            _jsonContext = new RedisContext(Config, new JsonSerializer());
            _msgPackContext = new RedisContext(Config, new MsgPack.MsgPackSerializer());
            _newtonsoftJsonContext = new RedisContext(Config, new NewtonsoftJson.NewtonsoftJsonSerializer());
#if (NET461)
            _binaryContext = new RedisContext(Config, new BinarySerializer());
            All = new[] { _binaryContext, _rawContext, _jsonContext, _msgPackContext, _newtonsoftJsonContext };
            BinAndRawAndJson = new[] { _binaryContext, _rawContext, _jsonContext, _newtonsoftJsonContext };
#else
            BinAndRawAndJson = new[] { _rawContext, _jsonContext, _newtonsoftJsonContext };
            All = new[] { _rawContext, _jsonContext, _msgPackContext, _newtonsoftJsonContext };
#endif

            Thread.Sleep(1500);
            _rawContext.Cache.FlushAll();
            // Get the redis version
            var server = _rawContext.GetConnectionMultiplexer().GetServer(_rawContext.GetConnectionMultiplexer().GetEndPoints()[0]);
            VersionInfo = server.Info("Server")[0].First(x => x.Key == "redis_version").Value.Split('.').Take(2).Select(x => int.Parse(x)).ToArray();
            // Enable keyspace notifications
            var eventsConfig = server.ConfigGet("notify-keyspace-events");
            if (!eventsConfig[0].Value.ToUpper().Contains('K') || !eventsConfig[0].Value.ToUpper().Contains('E'))
            {
                server.ConfigSet("notify-keyspace-events", "KEA");
            }
            
        }
    }
}
