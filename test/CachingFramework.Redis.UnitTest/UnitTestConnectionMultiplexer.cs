using NUnit.Framework;

namespace CachingFramework.Redis.UnitTest
{
    [TestFixture]
    public class UnitTestConnectionMultiplexer
    {
        [Test]
        public void Test_CustomMultiplexer()
        {
            var key = $"Test_CustomMultiplexer_obj-{Common.GetUId()}";
            var hash = $"Test_CustomMultiplexer_hash-{Common.GetUId()}";
            var myMultiplexer = new PooledConnectionMultiplexer(Common.Config);
            using (var ctx = new RedisContext(myMultiplexer))
            {
                ctx.Cache.SetObject(key, "Test_CustomMultiplexer_value");
                var list = ctx.Collections.GetRedisDictionary<string, string>(hash, 5);
                list.Add("test", "value");
            }

            using (var ctx = new RedisContext(myMultiplexer))
            {
                Assert.AreEqual("Test_CustomMultiplexer_value", ctx.Cache.GetObject<string>(key));
                var dict = ctx.Collections.GetRedisDictionary<string, string>(hash, 5);
                Assert.AreEqual("value", dict["test"]);
                ctx.Cache.Remove(key);
                ctx.Cache.Remove(hash);
            }
        }
    }
}
