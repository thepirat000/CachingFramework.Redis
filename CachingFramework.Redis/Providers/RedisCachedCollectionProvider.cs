using CachingFramework.Redis.Contracts.Providers;
using CachingFramework.Redis.Contracts.RedisObjects;
using CachingFramework.Redis.RedisObjects;

namespace CachingFramework.Redis.Providers
{
    /// <summary>
    /// Cached Collection provider using Redis
    /// </summary>
    internal class RedisCachedCollectionProvider : RedisProviderBase, ICachedCollectionProvider
    {
        #region Constructor
        /// <summary>
        /// Initializes a new instance of the <see cref="RedisCachedCollectionProvider"/> class.
        /// </summary>
        /// <param name="context">The context.</param>
        public RedisCachedCollectionProvider(RedisProviderContext context)
            : base(context)
        {
        }
        #endregion

        #region ICachedObjectsProvider Implementation
        /// <summary>
        /// Returns an IList implemented using a Redis List
        /// </summary>
        /// <typeparam name="T">The object type</typeparam>
        /// <param name="key">The redis key</param>
        public ICachedList<T> GetCachedList<T>(string key)
        {
            return new RedisList<T>(RedisConnection, key, Serializer);
        }
        /// <summary>
        /// Returns an IDictionary implemented using a Redis Hash
        /// </summary>
        /// <typeparam name="TKey">The key type</typeparam>
        /// <typeparam name="TValue">The object type</typeparam>
        /// <param name="key">The redis key</param>
        public ICachedDictionary<TKey, TValue> GetCachedDictionary<TKey, TValue>(string key)
        {
            return new RedisDictionary<TKey, TValue>(RedisConnection, key, Serializer);
        }
        /// <summary>
        /// Returns an ISet implemented using a Redis Set
        /// </summary>
        /// <typeparam name="T">The object type</typeparam>
        /// <param name="key">The redis key</param>
        public ICachedSet<T> GetCachedSet<T>(string key)
        {
            return new RedisSet<T>(RedisConnection, key, Serializer);
        }
        /// <summary>
        /// Returns an ICollection implemented using a Redis Sorted Set
        /// </summary>
        /// <typeparam name="T">The object type</typeparam>
        /// <param name="key">The redis key</param>
        public ICachedSortedSet<T> GetCachedSortedSet<T>(string key)
        {
            return new RedisSortedSet<T>(RedisConnection, key, Serializer);
        }
        /// <summary>
        /// Returns an ICollection implemented using a Redis string as a bitmap
        /// </summary>
        /// <param name="key">The redis key</param>
        public ICachedBitmap GetCachedBitmap(string key)
        {
            return new RedisBitmap(RedisConnection, key, Serializer);
        }
        /// <summary>
        /// Returns an ICollection(string) implemented using a Redis sorted set with lexicographical order
        /// </summary>
        /// <param name="key">The redis key</param>
        public ICachedLexicographicSet GetCachedLexicographicSet(string key)
        {
            return new RedisLexicographicSet(RedisConnection, key, Serializer);
        }
        #endregion
    }
}