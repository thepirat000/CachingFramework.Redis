using NUnit.Framework;
using NUnit.Framework.Legacy;

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
                Assert.That(ctx.Cache.GetObject<string>(key), Is.EqualTo("Test_CustomMultiplexer_value"));
                var dict = ctx.Collections.GetRedisDictionary<string, string>(hash, 5);
                Assert.That(dict["test"], Is.EqualTo("value"));
                ctx.Cache.Remove(key);
                ctx.Cache.Remove(hash);
            }
        }
    }
}
