using System;
using System.Configuration;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CachingFramework.Redis.Serializers;
using Microsoft.Extensions.Configuration;
using Nito.AsyncEx;
using StackExchange.Redis.KeyspaceIsolation;

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
        // A context using json and a prefix for the keys
        private static RedisContext _jsonPrefixedContext;
        // A context using newtonsoft json
        private static RedisContext _newtonsoftJsonContext;
        // A context using msgpack
        private static RedisContext _msgPackContext;
        // A context using memorypack
        private static RedisContext _memoryPackContext;


        // TestCases
        public static RedisContext[] JsonAndRaw { get { return new[] { _jsonContext, _rawContext, _newtonsoftJsonContext, _jsonPrefixedContext }; } }
        public static RedisContext[] Json { get { return new[] { _jsonContext, _newtonsoftJsonContext, _jsonPrefixedContext }; } }
        public static RedisContext[] JsonKeyPrefix { get { return new[] { _jsonPrefixedContext }; } }
        public static RedisContext[] NewtonsoftJson { get { return new[] { _newtonsoftJsonContext }; } }
        public static RedisContext[] MsgPack { get { return new[] { _msgPackContext }; } }
        public static RedisContext[] MemoryPack { get { return new[] { _memoryPackContext }; } }
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
            var configuration = new ConfigurationBuilder()
                                    .AddUserSecrets<UnitTestRedis>()
                                    .Build();

            var p = configuration["Password"];
            if (!string.IsNullOrEmpty(p))
                Config += $",password={p}";

            _rawContext = new RedisContext(Config, new RawSerializer());
            _jsonContext = new RedisContext(Config, new JsonSerializer());
            _jsonPrefixedContext = new RedisContext(Config, new JsonSerializer(), new DatabaseOptions { KeyPrefix = "PREFIX-" });
            _msgPackContext = new RedisContext(Config, new MsgPack.MsgPackSerializer());
            _newtonsoftJsonContext = new RedisContext(Config, new NewtonsoftJson.NewtonsoftJsonSerializer());
#if (NET462)
            _binaryContext = new RedisContext(Config, new BinarySerializer());
            All = new[] { _binaryContext, _rawContext, _jsonContext, _msgPackContext, _newtonsoftJsonContext, _jsonPrefixedContext };
            BinAndRawAndJson = new[] { _binaryContext, _rawContext, _jsonContext, _newtonsoftJsonContext, _jsonPrefixedContext };
#else
            _memoryPackContext = new RedisContext(Config, new MemoryPack.MemoryPackSerializer());
            BinAndRawAndJson = new[] { _rawContext, _jsonContext, _newtonsoftJsonContext, _jsonPrefixedContext };
            All = new[] { _rawContext, _jsonContext, _msgPackContext, _newtonsoftJsonContext, _jsonPrefixedContext, _memoryPackContext };
#endif

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
        public static async Task TestDeadlock(Action action)
        {
            var t = Task.Run(() =>
            {
                var singleThreadedSyncCtx = new AsyncContext().SynchronizationContext;
                SynchronizationContext.SetSynchronizationContext(singleThreadedSyncCtx);

                action();
            });
            await Task.Delay(TimeSpan.FromSeconds(1));
            if (!t.IsCompleted)
                throw new Exception("Code has deadlocked."); //usually means you have forgotten a ConfigureAwait(false)
        }

        public static string GetUId()
        {
            return Guid.NewGuid().ToString().Replace("-", "").Substring(0, 5);
        }
    }
}
