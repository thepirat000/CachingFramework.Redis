using CachingFramework.Redis.Contracts;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace CachingFramework.Redis.UnitTest
{
    [TestFixture]
    public class UnitTestTextAttributeCache
    {

        [Test]
        public void UT_TestCacheMapping()
        {
            var cache = new TextAttributeCache<KeyEvent>();
            Assert.That(cache.GetEnumText(KeyEvent.Delete), Is.EqualTo("del"));
            Assert.That(cache.GetEnumValue("del"), Is.EqualTo(KeyEvent.Delete));
            Assert.That(cache.GetEnumText(KeyEvent.Increment), Is.EqualTo("incrby"));
            Assert.That(cache.GetEnumValue("incrby"), Is.EqualTo(KeyEvent.Increment));
        }

        [Test]
        public void UT_TestStaticAccessor()
        {
            var cache = TextAttributeCache<KeyEvent>.Instance;
            Assert.That(cache.GetEnumText(KeyEvent.Delete), Is.EqualTo("del"));
            Assert.That(cache.GetEnumValue("del"), Is.EqualTo(KeyEvent.Delete));

            var cache1 = TextAttributeCache<Unit>.Instance;
            Assert.That(cache1.GetEnumText(Unit.Meters), Is.EqualTo("m"));
            Assert.That(cache1.GetEnumValue("m"), Is.EqualTo(Unit.Meters));
        }
    }
}
