namespace CachingFramework.Redis.Contracts
{
    /// <summary>
    /// Cached objects provider internal contract
    /// </summary>
    internal interface ICachedObjectsProvider
    {
        /// <summary>
        /// Returns an IList implemented using a Redis List
        /// </summary>
        /// <typeparam name="T">The object type</typeparam>
        /// <param name="key">The redis key</param>
        ICachedList<T> GetCachedList<T>(string key);
        /// <summary>
        /// Returns an IDictionary implemented using a Redis Hash
        /// </summary>
        /// <typeparam name="TKey">The key type</typeparam>
        /// <typeparam name="TValue">The object type</typeparam>
        /// <param name="key">The redis key</param>
        ICachedDictionary<TKey, TValue> GetCachedDictionary<TKey, TValue>(string key);
        /// <summary>
        /// Returns an ISet implemented using a Redis Set
        /// </summary>
        /// <typeparam name="T">The object type</typeparam>
        /// <param name="key">The redis key</param>
        ICachedSet<T> GetCachedSet<T>(string key);
    }
}