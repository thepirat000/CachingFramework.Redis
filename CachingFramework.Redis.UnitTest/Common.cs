using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using CachingFramework.Redis.Contracts;
using CachingFramework.Redis.Json;
using CachingFramework.Redis.Serializers;
using NUnit.Framework;

namespace CachingFramework.Redis.UnitTest
{
    public static class Common
    {
        // A context using a raw serializer
        private static Context _rawContext;
        // A context using a binary serializer
        private static Context _binaryContext;
        // A context using json
        private static Context _jsonContext;
        // A context using msgpack
        private static Context _msgPackContext;


        // TestCases
        public static Context[] Json { get { return new[] { _jsonContext }; } }
        public static Context[] MsgPack { get { return new[] { _msgPackContext }; } }
        public static Context[] Raw { get { return new[] { _rawContext }; } }
        public static Context[] Bin { get { return new[] { _binaryContext }; } }
        public static Context[] BinAndRaw { get { return new[] { _binaryContext, _rawContext }; } }
        public static Context[] BinAndRawAndJson { get { return new[] { _binaryContext, _rawContext, _jsonContext }; } }
        public static Context[] All { get { return new[] { _binaryContext, _rawContext, _jsonContext, _msgPackContext }; } }

        public static DateTime ServerNow
        {
            get
            {
                var cnn = _rawContext.GetConnectionMultiplexer();
                var serverNow = cnn.GetServer(cnn.GetEndPoints()[0]).Time();
                return serverNow;
            }
        }

        public static string Config = "192.168.15.10:6379, allowAdmin=true";
        //public static string Config = "192.168.15.13:7000, allowAdmin=true";
        //public static string Config = "192.168.15.11:7000, allowAdmin=true";

        static Common()
        {
            _rawContext = new Context(Config, new RawSerializer());
            _binaryContext = new Context(Config, new BinarySerializer());
            _jsonContext = new Json.Context(Config);
            _msgPackContext = new MsgPack.Context(Config);
            Thread.Sleep(1500);
            _rawContext.Cache.FlushAll();
        }
    }
}
