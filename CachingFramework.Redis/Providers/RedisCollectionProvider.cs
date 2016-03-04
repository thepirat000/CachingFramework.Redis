using CachingFramework.Redis.Contracts;
using CachingFramework.Redis.Contracts.Providers;
using CachingFramework.Redis.Contracts.RedisObjects;
using CachingFramework.Redis.RedisObjects;

namespace CachingFramework.Redis.Providers
{
    /// <summary>
    /// Collection provider using Redis
    /// </summary>
    internal class RedisCollectionProvider : RedisProviderBase, ICollectionProvider
    {
        #region Constructor
        /// <summary>
        /// Initializes a new instance of the <see cref="RedisCollectionProvider"/> class.
        /// </summary>
        /// <param name="context">The context.</param>
        public RedisCollectionProvider(RedisProviderContext context)
            : base(context)
        {
        }
        #endregion

        #region IRedisObjectsProvider Implementation
        /// <summary>
        /// Returns an IList implemented using a Redis List
        /// </summary>
        /// <typeparam name="T">The object type</typeparam>
        /// <param name="key">The redis key</param>
        /// <param name="serializer">The serializer to use, or NULL to use the context serializer</param>
        public IRedisList<T> GetRedisList<T>(string key, ISerializer serializer = null)
        {
            return new RedisList<T>(RedisConnection, key, serializer ?? Serializer);
        }
        /// <summary>
        /// Returns an IDictionary implemented using a Redis Hash
        /// </summary>
        /// <typeparam name="TKey">The key type</typeparam>
        /// <typeparam name="TValue">The object type</typeparam>
        /// <param name="key">The redis key</param>
        /// <param name="serializer">The serializer to use, or NULL to use the context serializer</param>
        public IRedisDictionary<TKey, TValue> GetRedisDictionary<TKey, TValue>(string key, ISerializer serializer = null)
        {
            return new RedisDictionary<TKey, TValue>(RedisConnection, key, serializer ?? Serializer);
        }
        /// <summary>
        /// Returns an ISet implemented using a Redis Set
        /// </summary>
        /// <typeparam name="T">The object type</typeparam>
        /// <param name="key">The redis key</param>
        /// <param name="serializer">The serializer to use, or NULL to use the context serializer</param>
        public IRedisSet<T> GetRedisSet<T>(string key, ISerializer serializer = null)
        {
            return new RedisSet<T>(RedisConnection, key, serializer ?? Serializer);
        }
        /// <summary>
        /// Returns an ICollection implemented using a Redis Sorted Set
        /// </summary>
        /// <typeparam name="T">The object type</typeparam>
        /// <param name="key">The redis key</param>
        /// <param name="serializer">The serializer to use, or NULL to use the context serializer</param>
        public IRedisSortedSet<T> GetRedisSortedSet<T>(string key, ISerializer serializer = null)
        {
            return new RedisSortedSet<T>(RedisConnection, key, serializer ?? Serializer);
        }
        /// <summary>
        /// Returns an ICollection implemented using a Redis string as a bitmap
        /// </summary>
        /// <param name="key">The redis key</param>
        public IRedisBitmap GetRedisBitmap(string key)
        {
            return new RedisBitmap(RedisConnection, key, Serializer);
        }
        /// <summary>
        /// Returns an ICollection(string) implemented using a Redis sorted set with lexicographical order
        /// </summary>
        /// <param name="key">The redis key</param>
        public IRedisLexicographicSet GetRedisLexicographicSet(string key)
        {
            return new RedisLexicographicSet(RedisConnection, key, Serializer);
        }
        /// <summary>
        /// Returns an ICollection(char) implemented using a Redis string
        /// </summary>
        /// <param name="key">The redis key</param>
        public IRedisString GetRedisString(string key)
        {
            return new RedisString(RedisConnection, key, Serializer);
        }
        #endregion
    }
}