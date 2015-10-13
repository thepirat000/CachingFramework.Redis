using System.Collections.Generic;
using CachingFramework.Redis.Contracts;
using CachingFramework.Redis.Contracts.RedisObjects;
using StackExchange.Redis;

namespace CachingFramework.Redis.RedisObjects
{
    /// <summary>
    /// Managed string using a Redis string.
    /// </summary>
    internal class RedisString : RedisBaseObject, ICachedString, IEnumerable<byte>
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
        /// Appends the value at the end of the string. 
        /// Returns the length of the string after the append operation.
        /// </summary>
        /// <param name="value">The value to append.</param>
        public long Append(string value)
        {
            return GetRedisDb().StringAppend(RedisKey, value);
        }
        /// <summary>
        /// Overwrites the entire string stored at key. 
        /// </summary>
        /// <param name="value">The new string to write.</param>
        /// <returns>The length of the string after it was modified by the command</returns>
        public long Set(string value)
        {
            return GetRedisDb().StringSet(RedisKey, value);
        }
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
        public long SetRange(long offset, string value)
        {
            return (long)GetRedisDb().StringSetRange(RedisKey, offset, value);
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
        /// <summary>
        /// Increments the number stored at key by <param name="increment"></param>.
        /// If the key does not exist, it is set to 0 before performing the operation.
        /// An exception is thrown if the key content can not be represented as integer.
        /// </summary>
        /// <param name="increment">The increment value</param>
        /// <returns>The value represented in the string after the increment.</returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public long IncrementBy(long increment)
        {
            return GetRedisDb().StringIncrement(RedisKey, increment);
        }
        /// <summary>
        /// Increment the string representing a floating point number stored at key by the specified increment.
        /// If the key does not exist, it is set to 0 before performing the operation.
        /// An exception is thrown if the key content is not parsable as a double precision floating point number.
        /// </summary>
        /// <param name="increment">The increment value</param>
        /// <returns>The value represented in the string after the increment.</returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public double IncrementByFloat(double increment)
        {
            return GetRedisDb().StringIncrement(RedisKey, increment);
        }
        #endregion
    }
}
