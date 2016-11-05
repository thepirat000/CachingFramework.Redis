using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CachingFramework.Redis.Contracts;
using CachingFramework.Redis.Contracts.RedisObjects;
using StackExchange.Redis;
using Bit = System.Byte;
using CachingFramework.Redis.Providers;

namespace CachingFramework.Redis.RedisObjects
{
    internal class RedisBitmap : RedisBaseObject, IRedisBitmap, ICollection<Bit>
    {
        #region Fields
        private const byte ByteSize = 8;
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="RedisBitmap" /> class.
        /// </summary>
        /// <param name="redisContext">The redis context.</param>
        /// <param name="redisKey">The redis key.</param>
        internal RedisBitmap(RedisProviderContext redisContext, string redisKey)
            : base(redisContext, redisKey)
        {
        }
        #endregion

        #region IRedisBitmap implementation
        /// <summary>
        /// Sets or clears the bit at offset. The bit is either set or cleared depending on bit parameter.
        /// When key does not exist, a new string value is created. The string is grown to make sure it can hold a bit at offset.
        /// </summary>
        /// <param name="offset">The zero-based offset.</param>
        /// <param name="bit">The bit value (any number > 0 is considered as true).</param>
        public void SetBit(long offset, Bit bit)
        {
            GetRedisDb().StringSetBit(RedisKey, offset, bit > 0);
        }
        /// <summary>
        /// Returns the bit value at offset in the string value stored at key. 
        /// When offset is beyond the string length, 0 is returned.
        /// </summary>
        /// <param name="offset">The zero-based offset.</param>
        public Bit GetBit(long offset)
        {
            return (Bit)(GetRedisDb().StringGetBit(RedisKey, offset) ? 1 : 0);
        }
        /// <summary>
        /// Return the position of the first bit set to 1 or 0 in the given byte range.  
        /// An start and end may be specified; these are in bytes, not bits; start and end can contain negative 
        /// values in order to index bytes starting from the end of the string.
        /// </summary>
        /// <param name="bit">The bit to search</param>
        /// <param name="start">The start position (in bytes)</param>
        /// <param name="stop">The end position (in bytes)</param>
        public long BitPosition(Bit bit, long start = 0, long stop = -1)
        {
            return GetRedisDb().StringBitPosition(RedisKey, bit > 0, start, stop);
            
        }
        /// <summary>
        /// Determines whether the bitmap contains the given bit within the byte(s) specified on the start/stop range.
        /// </summary>
        /// <param name="bit">The bit to check (any number > 0 is considered as true)</param>
        /// <param name="start">The start position (in bytes)</param>
        /// <param name="stop">The end position (in bytes)</param>
        public bool Contains(Bit bit, long start, long stop = -1)
        {
            var db = GetRedisDb();
            var pos = db.StringBitPosition(RedisKey, bit > 0, start, stop);
            return pos >= 0;
        }
        /// <summary>
        /// Gets the specified integer field of arbitrary size from the bitmap
        /// </summary>
        /// <param name="fieldType">Type of the field.</param>
        /// <param name="offset">The offset (bit or ordinal).</param>
        /// <param name="offsetIsOrdinal">if set to <c>true</c>, offset is ordinal, so offset=N means the N-th counter of the fieldType size.
        /// If set to <c>false</c>, offset is the bit position, so offset=N means the N-th bit</param>
        public T BitfieldGet<T>(BitfieldType fieldType, long offset, bool offsetIsOrdinal = false)
            where T : struct, IComparable, IComparable<T>, IConvertible, IEquatable<T>, IFormattable
        {
            var db = GetRedisDb();
            var args = new List<RedisValue>
            {
                "get",
                fieldType.ToString(),
                (offsetIsOrdinal ? "#" : "") + offset
            };
            var result = db.ScriptEvaluate(LuaScriptResource.Bitfield, new RedisKey[] { RedisKey }, args.ToArray());
            return (T)Convert.ChangeType((decimal)result, typeof(T));
        }
        /// <summary>
        /// Sets the specified integer field of arbitrary size in the bitmap
        /// </summary>
        /// <param name="fieldType">Type of the field.</param>
        /// <param name="offset">The offset (bit or ordinal).</param>
        /// <param name="value">The value to set.</param>
        /// <param name="offsetIsOrdinal">if set to <c>true</c>, offset is ordinal, so offset=N means the N-th counter of the fieldType size.
        /// If set to <c>false</c>, offset is the bit position, so offset=N means the N-th bit</param>
        /// <param name="overflowType">Overflow handling type.</param>
        /// <returns>The previous value.</returns>
        public T BitfieldSet<T>(BitfieldType fieldType, long offset, T value, bool offsetIsOrdinal = false, OverflowType overflowType = OverflowType.Wrap)
            where T : struct, IComparable, IComparable<T>, IConvertible, IEquatable<T>, IFormattable
        {
            var db = GetRedisDb();
            var args = new List<RedisValue>
            {
                "overflow",
                TextAttribute.GetEnumText(overflowType),
                "set",
                fieldType.ToString(),
                (offsetIsOrdinal ? "#" : "") + offset,
                Math.Round((decimal) Convert.ChangeType(value, typeof (decimal)), 0).ToString(CultureInfo.InvariantCulture)
            };
            var results = (RedisResult[])db.ScriptEvaluate(LuaScriptResource.Bitfield, new RedisKey[] {RedisKey}, args.ToArray());
            if (results[0].IsNull)
            {
                throw new OverflowException("The value would overflow the type " + fieldType);
            }
            return (T)Convert.ChangeType((decimal)results[0], typeof(T));
        }
        /// <summary>
        /// Increment the specified integer counter.
        /// </summary>
        /// <param name="fieldType">Type of the field.</param>
        /// <param name="offset">The offset (bit or ordinal).</param>
        /// <param name="increment">The value to increment.</param>
        /// <param name="offsetIsOrdinal">if set to <c>true</c>, offset is ordinal, so offset=N means the N-th counter of the fieldType size.
        /// If set to <c>false</c>, offset is the bit position, so offset=N means the N-th bit</param>
        /// <param name="overflowType">Overflow handling.</param>
        /// <returns>The previous value.</returns>
        public T BitfieldIncrementBy<T>(BitfieldType fieldType, long offset, T increment, bool offsetIsOrdinal = false, OverflowType overflowType = OverflowType.Wrap)
            where T : struct, IComparable, IComparable<T>, IConvertible, IEquatable<T>, IFormattable
        {
            var db = GetRedisDb();
            var args = new List<RedisValue>
            {
                "overflow",
                TextAttribute.GetEnumText(overflowType),
                "incrby",
                fieldType.ToString(),
                (offsetIsOrdinal ? "#" : "") + offset,
                Math.Round((decimal) Convert.ChangeType(increment, typeof (decimal)), 0).ToString(CultureInfo.InvariantCulture)
            };
            var results = (RedisResult[])db.ScriptEvaluate(LuaScriptResource.Bitfield, new RedisKey[] { RedisKey }, args.ToArray());
            if (results[0].IsNull)
            {
                throw new OverflowException("The value would overflow the type " + fieldType);
            }
            return (T)Convert.ChangeType((decimal)results[0], typeof(T));
        }
        #endregion

        #region ICollection implementation
        /// <summary>
        /// Appends an *entire byte* (8 bits) at the end of the bitmap, set to the given value.
        /// </summary>
        public void Add(byte @byte)
        {
            GetRedisDb().StringAppend(RedisKey, new[] { @byte });
        }
        /// <summary>
        /// Copies the entire bitmap to a compatible one-dimensional array, starting at the specified index of the target array.
        /// </summary>
        /// <param name="array">The one-dimensional Array that is the destination of the elements copied.</param>
        /// <param name="arrayIndex">The zero-based index in array at which copying begins.</param>
        public void CopyTo(byte[] array, int arrayIndex)
        {
            this.ToArray<byte>().CopyTo(array, arrayIndex);
        }
        /// <summary>
        /// Count the number of set bits (population counting) in the bitmap.  
        /// </summary>
        public int Count
        {
            get { return (int)BitCount(); }
        }
        /// <summary>
        /// Count the number of set bits (population counting) in the given byte range.  
        /// It is possible to specify the counting operation only in an interval passing the additional arguments start and end.  
        /// </summary>
        /// <param name="start">The start position (in bytes)</param>
        /// <param name="end">The end position (in bytes)</param>
        long IRedisBitmap.Count(long start, long end)
        {
            return BitCount(start, end);
        }
        /// <summary>
        /// Count the number of set bits (population counting) in the bitmap.  
        /// </summary>
        int ICollection<Bit>.Count
        {
            get { return (int)Count; }
        }
        /// <summary>
        /// Determines whether the bitmap contains the given bit.
        /// </summary>
        /// <param name="bit">The bit to check</param>
        bool ICollection<Bit>.Contains(Bit bit)
        {
            return Contains(bit, 0);
        }
        /// <summary>
        /// Inverts the first occurence of the specified bit in the bitmap.
        /// </summary>
        public bool Remove(Bit bit)
        {
            var db = GetRedisDb();
            long pos = db.StringBitPosition(RedisKey, bit > 0);
            if (pos >= 0)
            {
                db.StringSetBit(RedisKey, pos, bit == 0);
            }
            return pos >= 0;
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>A <see cref="T:System.Collections.Generic.IEnumerator`1" /> that can be used to iterate through the collection.</returns>
        public IEnumerator<Bit> GetEnumerator()
        {
            var db = GetRedisDb();
            var count = db.StringLength(RedisKey) * ByteSize;
            for(var i = 0; i < count; i++)
            {
                yield return (Bit)(db.StringGetBit(RedisKey, i) ? 1 : 0);
            }
        }
        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>An <see cref="T:System.Collections.IEnumerator" /> object that can be used to iterate through the collection.</returns>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        #endregion

        #region ToString override
        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>A <see cref="System.String" /> that represents this instance.</returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            foreach (Bit bit in this)
            {
                sb.Append(bit);
            }
            return sb.ToString();
        }
        #endregion

        #region Private methods
        /// <summary>
        /// Count the number of set bits in the bitmap.  
        /// </summary>
        private long BitCount(long start = 0, long stop = -1)
        {
            return GetRedisDb().StringBitCount(RedisKey, start, stop);
        }
        #endregion
    }
}
