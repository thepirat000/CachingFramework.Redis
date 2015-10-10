using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CachingFramework.Redis.Contracts;
using CachingFramework.Redis.Contracts.RedisObjects;
using StackExchange.Redis;

namespace CachingFramework.Redis.RedisObjects
{
    internal class RedisBitmap : RedisBaseObject, ICachedBitmap, ICollection<bool>
    {
        #region fields
        private const byte ByteSize = 8;
        #endregion
        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="RedisBitmap" /> class.
        /// </summary>
        /// <param name="connection">The connection.</param>
        /// <param name="redisKey">The redis key.</param>
        /// <param name="serializer">The serializer.</param>
        internal RedisBitmap(ConnectionMultiplexer connection, string redisKey, ISerializer serializer)
            : base(connection, redisKey, serializer)
        {
        }
        #endregion

        #region ICachedBitmap implementation
        /// <summary>
        /// Sets or clears the bit at offset. The bit is either set or cleared depending on <param name="bit"></param>.
        /// When key does not exist, a new string value is created. The string is grown to make sure it can hold a bit at offset.
        /// </summary>
        public void SetBit(long offset, bool bit)
        {
            GetRedisDb().StringSetBit(RedisKey, offset, bit);
        }
        /// <summary>
        /// Returns the bit value at offset in the string value stored at key. 
        /// When offset is beyond the string length, 0 is returned.
        /// </summary>
        /// <param name="offset">The zero-based offset.</param>
        public bool GetBit(long offset)
        {
            return GetRedisDb().StringGetBit(RedisKey, offset);
        }
        /// <summary>
        /// Return the position of the first bit set to 1 or 0 in the given byte range.  
        /// An start and end may be specified; these are in bytes, not bits; start and end can contain negative 
        /// values in order to index bytes starting from the end of the string.
        /// </summary>
        /// <param name="bit">The bit to search</param>
        /// <param name="start">The start position (in bytes)</param>
        /// <param name="stop">The end position (in bytes)</param>
        public long BitPosition(bool bit, long start = 0, long stop = -1)
        {
            return GetRedisDb().StringBitPosition(RedisKey, bit, start, stop);
            
        }
        /// <summary>
        /// Determines whether the bitmap contains the given bit within the byte(s) specified on the start/stop range.
        /// </summary>
        /// <param name="bit">The bit to check</param>
        /// <param name="start">The start position (in bytes)</param>
        /// <param name="stop">The end position (in bytes)</param>
        public bool Contains(bool bit, long start = 0, long stop = -1)
        {
            var db = GetRedisDb();
            var pos = db.StringBitPosition(RedisKey, bit, start, stop);
            var maxpos = db.StringLength(RedisKey) * ByteSize;
            return pos >= 0 && maxpos > 0 && pos < maxpos;
        }
        #endregion

        #region ICollection implementation
        /// <summary>
        /// Adds an *entire byte* (8 bits) to the bitmap with all bits set to the value given in <param name="value"></param>.
        /// </summary>
        public void Add(bool value)
        {
            GetRedisDb().StringAppend(RedisKey, new[] { value ? (byte)0xFF : (byte)0x00 });
        }
        /// <summary>
        /// Copies the entire bitmap to a compatible one-dimensional array, starting at the specified index of the target array.
        /// </summary>
        /// <param name="array">The one-dimensional Array that is the destination of the elements copied.</param>
        /// <param name="arrayIndex">The zero-based index in array at which copying begins.</param>
        public void CopyTo(bool[] array, int arrayIndex)
        {
            this.ToArray().CopyTo(array, arrayIndex);
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
        long ICachedBitmap.Count(long start, long end)
        {
            return BitCount(start, end);
        }
        /// <summary>
        /// Count the number of set bits (population counting) in the bitmap.  
        /// </summary>
        int ICollection<bool>.Count
        {
            get { return (int)Count; }
        }
        /// <summary>
        /// Determines whether the bitmap contains the given bit.
        /// </summary>
        /// <param name="bit">The bit to check</param>
        bool ICollection<bool>.Contains(bool bit)
        {
            return Contains(bit);
        }
        /// <summary>
        /// Inverts the first occurence of the specified bit in the bitmap.
        /// </summary>
        public bool Remove(bool bit)
        {
            var db = GetRedisDb();
            long pos = db.StringBitPosition(RedisKey, bit);
            if (pos >= 0)
            {
                db.StringSetBit(RedisKey, pos, !bit);
            }
            return pos >= 0;
        }
        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>A <see cref="T:System.Collections.Generic.IEnumerator`1" /> that can be used to iterate through the collection.</returns>
        public IEnumerator<bool> GetEnumerator()
        {
            var db = GetRedisDb();
            var count = db.StringLength(RedisKey) * ByteSize;
            for(var i = 0; i < count; i++)
            {
                yield return db.StringGetBit(RedisKey, i);
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
