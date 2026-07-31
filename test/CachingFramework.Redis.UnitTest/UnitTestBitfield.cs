using CachingFramework.Redis.Contracts;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace CachingFramework.Redis.UnitTest
{
    [TestFixture]
    public class UnitTestBitfield
    {
        [OneTimeSetUp]
        public void SetUpFixture()
        {
            if (Common.VersionInfo[0] < 3)
            {
                Assert.Ignore($"Bitfield tests ignored for version {string.Join(".", Common.VersionInfo)}\n");
            }
        }

        [Test, TestCaseSource(typeof(Common), nameof(Common.Raw))]
        public void UT_CacheBitmapBitField(RedisContext context)
        {
            var key = $"UT_CacheBitmapBitField-{Common.GetUId()}";
            context.Cache.Remove(key);
            var rb = context.Collections.GetRedisBitmap(key);
            var ex = rb.BitfieldSet(BitfieldType.u4, 0, 14, false, OverflowType.Fail);
            var n1 = rb.BitfieldGet<decimal>(BitfieldType.u4, 0);
            Assert.That(ex, Is.EqualTo(0));
            Assert.That(n1, Is.EqualTo(14));
            rb.BitfieldSet(BitfieldType.u16, 0, 0xb525);
            Assert.That(rb.BitfieldGet<int>(BitfieldType.u8, 0), Is.EqualTo(0xb5));
            Assert.That(rb.BitfieldGet<uint>(BitfieldType.u8, 1, true), Is.EqualTo(0x25));

            Assert.That(rb.BitfieldGet<byte>(BitfieldType.u3, 4), Is.EqualTo(0x2));
            Assert.That(rb.BitfieldGet<sbyte>(BitfieldType.u5, 7), Is.EqualTo(0x12));
            Assert.That(rb.BitfieldGet<long>(BitfieldType.u3, 2, true), Is.EqualTo(0x2));
        }

        [Test, TestCaseSource(typeof(Common), nameof(Common.Raw))]
        public void UT_CacheBitmapBitField_Overflow(RedisContext context)
        {
            var key = $"UT_CacheBitmapBitField_Overflow-{Common.GetUId()}";
            context.Cache.Remove(key);
            var rb = context.Collections.GetRedisBitmap(key);
            Assert.Throws<OverflowException>(() => rb.BitfieldSet(BitfieldType.u1, 0, -2, false, OverflowType.Fail));
            Assert.DoesNotThrow(() => rb.BitfieldSet(BitfieldType.u1, 0, -2));
            Assert.DoesNotThrow(() => rb.BitfieldSet(BitfieldType.u1, 0, -2, false, OverflowType.Saturation));
        }

        [Test, TestCaseSource(typeof(Common), nameof(Common.Raw))]
        public void UT_CacheBitmapBitField_WrapSaturation(RedisContext context)
        {
            var key = $"UT_CacheBitmapBitField_WrapSaturation-{Common.GetUId()}";
            context.Cache.Remove(key);
            var rb = context.Collections.GetRedisBitmap(key);
            rb.BitfieldSet(BitfieldType.u13, 10, 8191, true);
            Assert.That(rb.BitfieldGet<int>(BitfieldType.u13, 10, true), Is.EqualTo(8191));
            rb.BitfieldIncrementBy(BitfieldType.u13, 10, 4, true);
            Assert.That(rb.BitfieldGet<UInt16>(BitfieldType.u13, 10, true), Is.EqualTo(3));
            rb.BitfieldSet(BitfieldType.u13, 4, 8191);
            rb.BitfieldIncrementBy(BitfieldType.u13, 4, 999, false, OverflowType.Saturation);
            Assert.That(rb.BitfieldGet<UInt32>(BitfieldType.u13, 4), Is.EqualTo(8191));
        }
    }
}
