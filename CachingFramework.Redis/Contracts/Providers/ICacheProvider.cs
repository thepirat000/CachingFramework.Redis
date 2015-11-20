using System;
using System.Collections.Generic;

namespace CachingFramework.Redis.Contracts.Providers
{
    /// <summary>
    /// Cache Provider internal contract
    /// </summary>
    public interface ICacheProvider
    {
        /// <summary>
        /// Fetches hashed data from the cache, using the given cache key and field.
        /// If there is data in the cache with the given key, then that data is returned.
        /// If there is no such data in the cache (a cache miss occurred), then the value returned by func will be
        /// written to the cache under the given cache key-field, and that will be returned.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key">The cache key.</param>
        /// <param name="field">The field to obtain.</param>
        /// <param name="func">The function that returns the cache value, only executed when there is a cache miss.</param>
        /// <param name="expiry">The expiration timespan.</param>
        /// <returns>``0.</returns>
        T FetchHashed<T>(string key, string field, Func<T> func, TimeSpan? expiry = null);
        /// <summary>
        /// Fetches data from the cache, using the given cache key.
        /// If there is data in the cache with the given key, then that data is returned.
        /// If there is no such data in the cache (a cache miss occurred), then the value returned by func will be
        /// written to the cache under the given cache key and associated to the given tags, and that will be returned.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key">The cache key.</param>
        /// <param name="func">The function that returns the cache value, only executed when there is a cache miss.</param>
        /// <param name="expiry">The expiration timespan.</param>
        T FetchObject<T>(string key, Func<T> func, TimeSpan? expiry = null);
        /// <summary>
        /// Fetches data from the cache, using the given cache key.
        /// If there is data in the cache with the given key, then that data is returned.
        /// If there is no such data in the cache (a cache miss occurred), then the value returned by func will be
        /// written to the cache under the given cache key and associated to the given tags, and that will be returned.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key">The cache key.</param>
        /// <param name="func">The function that returns the cache value, only executed when there is a cache miss.</param>
        /// <param name="tags">The tags to associate with the key. Only associated when there is a cache miss.</param>
        /// <param name="expiry">The expiration timespan.</param>
        /// <returns>``0.</returns>
        T FetchObject<T>(string key, Func<T> func, string[] tags, TimeSpan? expiry = null);
        /// <summary>
        /// Set the value of a key
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <param name="ttl">The time to live.</param>
        void SetObject<T>(string key, T value, TimeSpan? ttl = null);
        /// <summary>
        /// Set the value of a key, associating the key with the given tag(s).
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <param name="tags">The tags.</param>
        /// <param name="ttl">The time to live.</param>
        void SetObject<T>(string key, T value, string[] tags, TimeSpan? ttl = null);
        /// <summary>
        /// Atomically sets key to value and returns the old value stored at key. 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key">The key.</param>
        /// <param name="value">The new value value.</param>
        /// <returns>The old value</returns>
        T GetSetObject<T>(string key, T value);
        /// <summary>
        /// Relates the given tags to a key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="tags">The tag(s).</param>
        void AddTagsToKey(string key, string[] tags);
        /// <summary>
        /// Removes the relation between the given tags and a key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="tags">The tag(s).</param>
        void RemoveTagsFromKey(string key, string[] tags);
        /// <summary>
        /// Get the value of a key
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key">The key.</param>
        /// <returns>``0.</returns>
        T GetObject<T>(string key);
        /// <summary>
        /// Try to get the value of a key
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value"> When this method returns, contains the value associated with the specified key, if the key is found; 
        /// otherwise, the default value for the type of the value parameter.</param>
        /// <returns>True if the cache contains an element with the specified key; otherwise, false.</returns>
        bool TryGetObject<T>(string key, out T value);
        /// <summary>
        /// Gets all the keys related to the given tag(s).
        /// Returns a hashset with the keys.
        /// </summary>
        /// <param name="tags">The tags.</param>
        /// <param name="cleanUp">True to return only the existing keys within the tags (slower). Default is false.</param>
        /// <returns>HashSet{System.String}.</returns>
        ISet<string> GetKeysByTag(string[] tags, bool cleanUp = false);
        /// <summary>
        /// Returns all the objects that has the given tag(s) related.
        /// Assumes all the objects are of the same type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The objects types</typeparam>
        /// <param name="tags">The tags</param>
        IEnumerable<T> GetObjectsByTag<T>(params string[] tags);
        /// <summary>
        /// Removes all the keys related to the given tag(s).
        /// </summary>
        /// <param name="tags">The tags.</param>
        void InvalidateKeysByTag(params string[] tags);
        /// <summary>
        /// Returns the entire collection of tags
        /// </summary>
        ISet<string> GetAllTags();
        /// <summary>
        /// Determines if a key exists.
        /// </summary>
        /// <param name="key">The key.</param>
        bool KeyExists(string key);
        /// <summary>
        /// Sets the expiration of a key from a local date time expiration value.
        /// </summary>
        /// <param name="key">The key to expire</param>
        /// <param name="expiration">The expiration local date time</param>
        /// <returns>True is the key expiration was updated</returns>
        bool KeyExpire(string key, DateTime expiration);
        /// <summary>
        /// Sets the time-to-live of a key from a timespan value.
        /// </summary>
        /// <param name="key">The key to expire</param>
        /// <param name="ttl">The TTL timespan</param>
        /// <returns>True is the key expiration was updated</returns>
        bool KeyTimeToLive(string key, TimeSpan ttl);
        /// <summary>
        /// Removes the expiration of the given key.
        /// </summary>
        /// <param name="key">The key to persist</param>
        /// <returns>True is the key expiration was removed</returns>
        bool KeyPersist(string key);
        /// <summary>
        /// Removes the specified keys.
        /// </summary>
        /// <param name="keys">The keys to remove.</param>
        void Remove(string[] keys);
        /// <summary>
        /// Removes the specified key-value.
        /// </summary>
        /// <param name="key">The key.</param>
        bool Remove(string key);
        /// <summary>
        /// Sets the specified value to a hashset using the pair hashKey+field.
        /// (The latest expiration applies to the whole key)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key">The key.</param>
        /// <param name="field">The field key</param>
        /// <param name="value">The value to store</param>
        /// <param name="ttl">Set the current expiration timespan to the whole key (not only this hash). NULL to keep the current expiration.</param>
        void SetHashed<T>(string key, string field, T value, TimeSpan? ttl = null);
        /// <summary>
        /// Sets the specified key/values pairs to a hashset.
        /// (The latest expiration applies to the whole key)
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="fieldValues">The field keys and values to store</param>
        /// <param name="ttl">Set the current expiration timespan to the whole key (not only this hash). NULL to keep the current expiration.</param>
        void SetHashed<T>(string key, IDictionary<string, T> fieldValues, TimeSpan? ttl = null);
        /// <summary>
        /// Gets a specified hased value from a key
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key">The key.</param>
        /// <param name="field">The field.</param>
        /// <returns>``0.</returns>
        T GetHashed<T>(string key, string field);
        /// <summary>
        /// Try to get the value of a hash key
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="field">The hash field.</param>
        /// <param name="value"> When this method returns, contains the value associated with the specified hash field within the key, if the key and field are found; 
        /// otherwise, the default value for the type of the value parameter.</param>
        /// <returns>True if the cache contains a hashed element with the specified key and field; otherwise, false.</returns>
        bool TryGetHashed<T>(string key, string field, out T value);
        /// <summary>
        /// Removes a specified hased value from cache
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="field">The field.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        bool RemoveHashed(string key, string field);
        /// <summary>
        /// Gets all the values from a hash.
        /// The keys of the dictionary are the field names and the values are the objects
        /// </summary>
        /// <param name="key">The key.</param>
        IDictionary<string, T> GetHashedAll<T>(string key);
        /// <summary>
        /// Flushes all the databases on every master node.
        /// </summary>
        void FlushAll();
        /// <summary>
        /// Adds all the element arguments to the HyperLogLog data structure stored at the specified key.
        /// </summary>
        /// <typeparam name="T">The items type</typeparam>
        /// <param name="key">The redis key.</param>
        /// <param name="items">The items to add.</param>
        /// <returns><c>true</c> if at least 1 HyperLogLog internal register was altered, <c>false</c> otherwise.</returns>
        bool HyperLogLogAdd<T>(string key, T[] items);
        /// <summary>
        /// Adds the element to the HyperLogLog data structure stored at the specified key.
        /// </summary>
        /// <typeparam name="T">The items type</typeparam>
        /// <param name="key">The redis key.</param>
        /// <param name="item">The item to add.</param>
        /// <returns><c>true</c> if at least 1 HyperLogLog internal register was altered, <c>false</c> otherwise.</returns>
        bool HyperLogLogAdd<T>(string key, T item);
        /// <summary>
        /// Returns the approximated cardinality computed by the HyperLogLog data structure stored at the specified key, which is 0 if the variable does not exist.
        /// </summary>
        /// <param name="key">The redis key.</param>
        long HyperLogLogCount(string key);
    }
}
