using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using CachingFramework.Redis.Contracts;
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
        
        // TestCases
        public static Context[] Raw { get { return new[] { _rawContext }; } }
        public static Context[] Bin { get { return new[] { _binaryContext }; } }
        public static Context[] All { get { return new[] { _binaryContext, _rawContext }; } }

        static Common()
        {
            var config = "192.168.15.11:6379, allowAdmin=true";
            //var config = "192.168.15.15:7000, allowAdmin=true";
            //var config = "192.168.15.11:7000, allowAdmin=true";
            _rawContext = new Context(config, new RawSerializer());
            _binaryContext = new Context(config, new BinarySerializer());
            Thread.Sleep(1500);
            _rawContext.Cache.FlushAll();
        }
    }
}
