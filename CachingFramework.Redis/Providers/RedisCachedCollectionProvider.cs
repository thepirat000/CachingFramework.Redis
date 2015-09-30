using CachingFramework.Redis.Contracts;
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
            return new RedisHashSet<T>(RedisConnection, key, Serializer);
        }
        #endregion
    }
}