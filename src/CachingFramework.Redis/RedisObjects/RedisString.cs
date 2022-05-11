using System.Collections.Generic;
using CachingFramework.Redis.Contracts;
using CachingFramework.Redis.Contracts.RedisObjects;
using StackExchange.Redis;
using CachingFramework.Redis.Providers;
using System.Threading.Tasks;

namespace CachingFramework.Redis.RedisObjects
{
    /// <summary>
    /// Managed string using a Redis string.
    /// </summary>
    internal class RedisString : RedisBaseObject, IRedisString, IRedisStringAsync, IEnumerable<byte>
    {
        #region Constructors
        /// <inheritdoc />
        internal RedisString(RedisProviderContext redisContext, string redisKey)
            : base(redisContext, redisKey)
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

        #region IRedisString implementation
        /// <inheritdoc />
        public long Append(string value)
        {
            return GetRedisDb().StringAppend(RedisKey, value);
        }
        /// <inheritdoc />
        public async Task<long> AppendAsync(string value)
        {
            return await GetRedisDb().StringAppendAsync(RedisKey, value).ConfigureAwait(false);
        }
        /// <inheritdoc />
        public void Set(string value)
        {
            GetRedisDb().StringSet(RedisKey, value);
        }
        /// <inheritdoc />
        public async Task SetAsync(string value)
        {
            await GetRedisDb().StringSetAsync(RedisKey, value).ConfigureAwait(false);
        }
        /// <inheritdoc />
        public void Set(long value)
        {
            GetRedisDb().StringSet(RedisKey, value);
            
        }
        /// <inheritdoc />
        public async Task SetAsync(long value)
        {
            await GetRedisDb().StringSetAsync(RedisKey, value).ConfigureAwait(false);
        }
        /// <inheritdoc />
        public void Set(double value)
        {
            GetRedisDb().StringSet(RedisKey, value);
        }
        /// <inheritdoc />
        public async Task SetAsync(double value)
        {
            await GetRedisDb().StringSetAsync(RedisKey, value).ConfigureAwait(false);
        }
        /// <inheritdoc />
        public string GetSet(string value)
        {
            return GetRedisDb().StringGetSet(RedisKey, value);
        }
        /// <inheritdoc />
        public async Task<string> GetSetAsync(string value)
        {
            return await GetRedisDb().StringGetSetAsync(RedisKey, value).ConfigureAwait(false);
        }
        /// <inheritdoc />
        public long GetSet(long value)
        {
            return (long)GetRedisDb().StringGetSet(RedisKey, value);
        }
        /// <inheritdoc />
        public async Task<long> GetSetAsync(long value)
        {
            return (long) await GetRedisDb().StringGetSetAsync(RedisKey, value);
        }
        /// <inheritdoc />
        public double GetSet(double value)
        {
            return (double)GetRedisDb().StringGetSet(RedisKey, value);
        }
        /// <inheritdoc />
        public async Task<double> GetSetAsync(double value)
        {
            return (double) await GetRedisDb().StringGetSetAsync(RedisKey, value).ConfigureAwait(false);
        }
        /// <inheritdoc />
        public string this[long start, long stop] => GetRange(start, stop);

        /// <summary>
        /// Returns the string that represents this instance.
        /// </summary>
        public override string ToString()
        {
            return GetRedisDb().StringGet(RedisKey);
        }
        /// <inheritdoc />
        public async Task<string> ToStringAsync()
        {
            return await GetRedisDb().StringGetAsync(RedisKey).ConfigureAwait(false);
        }
        /// <inheritdoc />
        public long SetRange(long offset, string value)
        {
            return (long)GetRedisDb().StringSetRange(RedisKey, offset, value);
        }
        /// <inheritdoc />
        public async Task<long> SetRangeAsync(long offset, string value)
        {
            return (long)await GetRedisDb().StringSetRangeAsync(RedisKey, offset, value).ConfigureAwait(false);
        }
        /// <inheritdoc />
        public string GetRange(long start = 0, long stop = -1)
        {
            return GetRedisDb().StringGetRange(RedisKey, start, stop);
        }
        /// <inheritdoc />
        public async Task<string> GetRangeAsync(long start = 0, long stop = -1)
        {
            return await GetRedisDb().StringGetRangeAsync(RedisKey, start, stop).ConfigureAwait(false);
        }
        /// <inheritdoc />
        public long Length => GetRedisDb().StringLength(RedisKey);

        /// <inheritdoc />
        public long IncrementBy(long increment)
        {
            return GetRedisDb().StringIncrement(RedisKey, increment);
        }
        /// <inheritdoc />
        public async Task<long> IncrementByAsync(long increment)
        {
            return await GetRedisDb().StringIncrementAsync(RedisKey, increment).ConfigureAwait(false);
        }
        /// <inheritdoc />
        public double IncrementByFloat(double increment)
        {
            return GetRedisDb().StringIncrement(RedisKey, increment);
        }
        /// <inheritdoc />
        public async Task<double> IncrementByFloatAsync(double increment)
        {
            return await GetRedisDb().StringIncrementAsync(RedisKey, increment).ConfigureAwait(false);
        }
        /// <inheritdoc />
        public long AsInteger(long @default = default(long))
        {
            var value = GetRedisDb().StringGet(RedisKey);
            return value.IsNull ? @default : (long)value;
        }
        /// <inheritdoc />
        public async Task<long> AsIntegerAsync(long @default = default(long))
        {
            var value = await GetRedisDb().StringGetAsync(RedisKey).ConfigureAwait(false);
            return value.IsNull ? @default : (long)value;
        }
        /// <inheritdoc />
        public double AsFloat(double @default = default(double))
        {
            var value = GetRedisDb().StringGet(RedisKey);
            return value.IsNull ? @default : (double)value;
        }
        /// <inheritdoc />
        public async Task<double> AsFloatAsync(double @default = default(double))
        {
            var value = await GetRedisDb().StringGetAsync(RedisKey).ConfigureAwait(false);
            return value.IsNull ? @default : (double)value;
        }
        #endregion
    }
}
