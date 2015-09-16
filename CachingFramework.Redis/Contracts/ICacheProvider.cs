using System;
using System.Collections.Generic;
using StackExchange.Redis;

namespace CachingFramework.Redis.Contracts
{
    /// <summary>
    /// Cache Provider internal contract
    /// </summary>
    internal interface ICacheProvider
    {
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
        /// Gets all the keys related to the given tag(s).
        /// Returns a hashset with the keys.
        /// </summary>
        /// <param name="tags">The tags.</param>
        /// <param name="cleanUp">True to return only the existing keys within the tags (slower). Default is false.</param>
        /// <returns>HashSet{System.String}.</returns>
        HashSet<string> GetKeysByTag(string[] tags, bool cleanUp = false);
        /// <summary>
        /// Returns all the objects that has the given tag(s) related.
        /// Assumes all the objects are of the same type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The objects types</typeparam>
        /// <param name="tags">The tags</param>
        IEnumerable<T> GetObjectsByTag<T>(string[] tags);
        /// <summary>
        /// Removes all the keys related to the given tag(s).
        /// </summary>
        /// <param name="tags">The tags.</param>
        void InvalidateKeysByTag(string[] tags);
        /// <summary>
        /// Removes the specified key-value.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
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
        void SetHashed<T>(string key, IDictionary<string, object> fieldValues, TimeSpan? ttl = null);
        /// <summary>
        /// Gets a specified hased value from a key
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key">The key.</param>
        /// <param name="field">The field.</param>
        /// <returns>``0.</returns>
        T GetHashed<T>(string key, string field);
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
    }
}
