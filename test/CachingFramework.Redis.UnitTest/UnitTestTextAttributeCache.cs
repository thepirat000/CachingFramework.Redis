using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CachingFramework.Redis.Contracts;
using NUnit.Framework;

namespace CachingFramework.Redis.UnitTest
{
    [TestFixture]
    public class UnitTestTextAttributeCache
    {

        [Test]
        public void UT_TestCacheMapping()
        {
            var cache = new TextAttributeCache<KeyEvent>();
            Assert.AreEqual("del", cache.GetEnumText(KeyEvent.Delete));
            Assert.AreEqual(KeyEvent.Delete, cache.GetEnumValue("del"));
            Assert.AreEqual("incrby", cache.GetEnumText(KeyEvent.Increment));
            Assert.AreEqual(KeyEvent.Increment, cache.GetEnumValue("incrby"));
        }

        [Test]
        public void UT_TestStaticAccessor()
        {
            var cache = TextAttributeCache<KeyEvent>.Instance;
            Assert.AreEqual("del", cache.GetEnumText(KeyEvent.Delete));
            Assert.AreEqual(KeyEvent.Delete, cache.GetEnumValue("del"));

            var cache1 = TextAttributeCache<Unit>.Instance;
            Assert.AreEqual("m", cache1.GetEnumText(Unit.Meters));
            Assert.AreEqual(Unit.Meters, cache1.GetEnumValue("m"));
        }
    }
}
