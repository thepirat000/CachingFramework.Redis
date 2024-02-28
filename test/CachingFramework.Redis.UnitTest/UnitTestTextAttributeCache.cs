using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CachingFramework.Redis.Contracts;
using NUnit.Framework;using NUnit.Framework.Legacy;

namespace CachingFramework.Redis.UnitTest
{
    [TestFixture]
    public class UnitTestTextAttributeCache
    {

        [Test]
        public void UT_TestCacheMapping()
        {
            var cache = new TextAttributeCache<KeyEvent>();
            ClassicAssert.AreEqual("del", cache.GetEnumText(KeyEvent.Delete));
            ClassicAssert.AreEqual(KeyEvent.Delete, cache.GetEnumValue("del"));
            ClassicAssert.AreEqual("incrby", cache.GetEnumText(KeyEvent.Increment));
            ClassicAssert.AreEqual(KeyEvent.Increment, cache.GetEnumValue("incrby"));
        }

        [Test]
        public void UT_TestStaticAccessor()
        {
            var cache = TextAttributeCache<KeyEvent>.Instance;
            ClassicAssert.AreEqual("del", cache.GetEnumText(KeyEvent.Delete));
            ClassicAssert.AreEqual(KeyEvent.Delete, cache.GetEnumValue("del"));

            var cache1 = TextAttributeCache<Unit>.Instance;
            ClassicAssert.AreEqual("m", cache1.GetEnumText(Unit.Meters));
            ClassicAssert.AreEqual(Unit.Meters, cache1.GetEnumValue("m"));
        }
    }
}
