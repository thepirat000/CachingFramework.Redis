using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using CachingFramework.Redis.Serializers;

namespace CachingFramework.Redis.UnitTest
{
    [TestFixture]
    public class UnitTestJson
    {

#if (NET45 || NET461)
        [Test]
        public void UT_JsonGet()
        {
            var ctx = new Context(Common.Config, new JsonSerializer());
            string key = "UT_JsonGet";
            ctx.Cache.Remove(key);
            ArgumentOutOfRangeException realEx;
            try
            {
                throw new ArgumentOutOfRangeException("param", new FileNotFoundException("test"));
            }
            catch (ArgumentOutOfRangeException ex)
            {
                realEx = ex;
                ctx.Cache.SetObject(key, ex);
            }
            var exception = ctx.Cache.GetObject<Exception>(key);
            Assert.AreEqual(realEx.StackTrace, exception.StackTrace);
            Assert.AreEqual(realEx.Message, exception.Message);
            Assert.AreEqual(realEx.InnerException.Message, exception.InnerException.Message);
        }
#endif
    }
}