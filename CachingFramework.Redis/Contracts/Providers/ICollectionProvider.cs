using CachingFramework.Redis.Contracts.RedisObjects;

namespace CachingFramework.Redis.Contracts.Providers
{
    /// <summary>
    /// Cached objects provider internal contract
    /// </summary>
    public interface ICollectionProvider
    {
        /// <summary>
        /// Returns an ICachedList implemented using a Redis List
        /// </summary>
        /// <typeparam name="T">The object type</typeparam>
        /// <param name="key">The redis key</param>
        ICachedList<T> GetCachedList<T>(string key);
        /// <summary>
        /// Returns an ICachedDictionary implemented using a Redis Hash
        /// </summary>
        /// <typeparam name="TKey">The key type</typeparam>
        /// <typeparam name="TValue">The object type</typeparam>
        /// <param name="key">The redis key</param>
        ICachedDictionary<TKey, TValue> GetCachedDictionary<TKey, TValue>(string key);
        /// <summary>
        /// Returns an ICachedSet implemented using a Redis Set
        /// </summary>
        /// <typeparam name="T">The object type</typeparam>
        /// <param name="key">The redis key</param>
        ICachedSet<T> GetCachedSet<T>(string key);
        /// <summary>
        /// Returns an ICachedSortedSet implemented using a Redis Sorted Set
        /// </summary>
        /// <typeparam name="T">The object type</typeparam>
        /// <param name="key">The redis key</param>
        ICachedSortedSet<T> GetCachedSortedSet<T>(string key);
        /// <summary>
        /// Returns an ICollection implemented using a Redis string as a bitmap
        /// </summary>
        /// <param name="key">The redis key</param>
        ICachedBitmap GetCachedBitmap(string key);
        /// <summary>
        /// Returns an ICollection(string) implemented using a Redis sorted set with lexicographical order
        /// </summary>
        /// <param name="key">The redis key</param>
        ICachedLexicographicSet GetCachedLexicographicSet(string key);
        /// <summary>
        /// Returns an ICollection(char) implemented using a Redis string
        /// </summary>
        /// <param name="key">The redis key</param>
        ICachedString GetCachedString(string key);
    }
}