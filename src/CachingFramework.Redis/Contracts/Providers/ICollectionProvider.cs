using CachingFramework.Redis.Contracts.RedisObjects;

namespace CachingFramework.Redis.Contracts.Providers
{
    /// <summary>
    /// Cached objects provider internal contract
    /// </summary>
    public interface ICollectionProvider
    {
        /// <summary>
        /// Returns an IRedisList implemented using a Redis List
        /// </summary>
        /// <typeparam name="T">The object type</typeparam>
        /// <param name="key">The redis key</param>
        /// <param name="serializer">The serializer to use, or NULL to use the context serializer</param>
        IRedisList<T> GetRedisList<T>(string key);
        /// <summary>
        /// Returns an IRedisDictionary implemented using a Redis Hash
        /// </summary>
        /// <typeparam name="TKey">The key type</typeparam>
        /// <typeparam name="TValue">The object type</typeparam>
        /// <param name="key">The redis key</param>
        /// <param name="serializer">The serializer to use, or NULL to use the context serializer</param>
        IRedisDictionary<TKey, TValue> GetRedisDictionary<TKey, TValue>(string key);
        /// <summary>
        /// Returns an IRedisSet implemented using a Redis Set
        /// </summary>
        /// <typeparam name="T">The object type</typeparam>
        /// <param name="key">The redis key</param>
        /// <param name="serializer">The serializer to use, or NULL to use the context serializer</param>
        IRedisSet<T> GetRedisSet<T>(string key);
        /// <summary>
        /// Returns an IRedisSortedSet implemented using a Redis Sorted Set
        /// </summary>
        /// <typeparam name="T">The object type</typeparam>
        /// <param name="key">The redis key</param>
        /// <param name="serializer">The serializer to use, or NULL to use the context serializer</param>
        IRedisSortedSet<T> GetRedisSortedSet<T>(string key);
        /// <summary>
        /// Returns an ICollection implemented using a Redis string as a bitmap
        /// </summary>
        /// <param name="key">The redis key</param>
        IRedisBitmap GetRedisBitmap(string key);
        /// <summary>
        /// Returns an ICollection(string) implemented using a Redis sorted set with lexicographical order
        /// </summary>
        /// <param name="key">The redis key</param>
        IRedisLexicographicSet GetRedisLexicographicSet(string key);
        /// <summary>
        /// Returns an ICollection(char) implemented using a Redis string
        /// </summary>
        /// <param name="key">The redis key</param>
        IRedisString GetRedisString(string key);
    }
}