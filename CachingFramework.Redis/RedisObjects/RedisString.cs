using System.Collections.Generic;
using CachingFramework.Redis.Contracts;
using CachingFramework.Redis.Contracts.RedisObjects;
using StackExchange.Redis;

namespace CachingFramework.Redis.RedisObjects
{
    /// <summary>
    /// Managed string using a Redis string.
    /// </summary>
    internal class RedisString : RedisBaseObject, ICachedString
    {
        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="RedisBaseObject" /> class.
        /// </summary>
        /// <param name="connection">The connection.</param>
        /// <param name="redisKey">The redis key.</param>
        /// <param name="serializer">The serializer (not used in this class).</param>
        internal RedisString(ConnectionMultiplexer connection, string redisKey, ISerializer serializer)
            : base(connection, redisKey, serializer)
        {
        }
        #endregion

        #region IEnumerable implementation
        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>A <see cref="T:System.Collections.Generic.IEnumerator`1" /> that can be used to iterate through the collection.</returns>
        public IEnumerator<byte> GetEnumerator()
        {
            var db = GetRedisDb();
            var length = db.StringLength(RedisKey);
            for (long i = 0; i < length; i++)
            {
                var value = (byte[])db.StringGetRange(RedisKey, i, i);
                yield return value[0];
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

        #region ICachedString implementation
        /// <summary>
        /// Gets the <see cref="System.String"/> with the specified start.
        /// </summary>
        /// <param name="start">The start.</param>
        /// <param name="stop">The stop.</param>
        /// <returns>System.String.</returns>
        public string this[long start, long stop]
        {
            get { return GetRange(start, stop); }
        }
        /// <summary>
        /// Returns the string that represents this instance.
        /// </summary>
        public override string ToString()
        {
            return GetRedisDb().StringGet(RedisKey);
        }
        /// <summary>
        /// Overwrites part of the string stored at key, starting at the specified offset, for the entire length of value.
        /// If the offset is larger than the current length of the string at key, the string is padded with zero-bytes to make offset fit.
        /// Non-existing keys are considered as empty strings, so this command will make sure it holds a string large enough to be able to set value at offset.
        /// </summary>
        /// <param name="offset">The zero-based offset in bytes.</param>
        /// <param name="item">The string to write.</param>
        /// <returns>The length of the string after it was modified by the command</returns>
        public long SetRange(long offset, string item)
        {
            return (long)GetRedisDb().StringSetRange(RedisKey, offset, item);
        }
        /// <summary>
        /// Returns the substring of the string value stored at key, determined by the offsets start and stop (both are inclusive).
        /// Negative offsets can be used in order to provide an offset starting from the end of the string. So -1 means the last character.
        /// </summary>
        /// <param name="start">The start zero-based index (can be negative number indicating offset from the end of the sorted set).</param>
        /// <param name="stop">The stop zero-based index (can be negative number indicating offset from the end of the sorted set).</param>
        /// <returns>System.String.</returns>
        public string GetRange(long start = 0, long stop = -1)
        {
            return GetRedisDb().StringGetRange(RedisKey, start, stop);
        }
        /// <summary>
        /// Returns the length of the string value stored at key.
        /// </summary>
        /// <value>The length.</value>
        public long Length
        {
            get
            {
                return GetRedisDb().StringLength(RedisKey);
            }
        }
        #endregion
    }
}
