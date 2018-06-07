using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CachingFramework.Redis.Contracts.Providers
{
    /// <summary>
    /// Cache Provider async contract
    /// </summary>
    public interface ICacheProviderAsync
    {
        /// <summary>
        /// Relates the given tags to a key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="tags">The tag(s).</param>
        Task AddTagsToKeyAsync(string key, string[] tags);
        /// <summary>
        /// Relates the given tags to a field inside a hash key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="field">The field.</param>
        /// <param name="tags">The tag(s).</param>
        Task AddTagsToHashFieldAsync(string key, string field, string[] tags);
        /// <summary>
        /// Removes the relation between the given tags and a field in a hash.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="field">The field.</param>
        /// <param name="tags">The tag(s).</param>
        Task RemoveTagsFromHashFieldAsync(string key, string field, string[] tags);
        /// <summary>
        /// Removes the relation between the given tags and a key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="tags">The tag(s).</param>
        Task RemoveTagsFromKeyAsync(string key, string[] tags);
        /// <summary>
        /// Removes the relation between the given tags and a set member.
        /// </summary>
        /// <param name="key">The set key.</param>
        /// <param name="member">The set member related to the tags.</param>
        /// <param name="tags">The tag(s).</param>
        Task RemoveTagsFromSetMemberAsync<T>(string key, T member, string[] tags);
        /// <summary>
        /// Relates the given tags to a member inside a redis set, sorted set or geospatial index.
        /// </summary>
        /// <param name="key">The redis set, sorted set or geospatial index key.</param>
        /// <param name="member">The set member.</param>
        /// <param name="tags">The tag(s).</param>
        Task AddTagsToSetMemberAsync<T>(string key, T member, string[] tags);
        /// <summary>
        /// Adds the given value to a redis set.
        /// (The latest expiration applies to the whole key)
        /// </summary>
        /// <typeparam name="T">The member type</typeparam>
        /// <param name="key">The redis set key.</param>
        /// <param name="value">The member value to store</param>
        /// <param name="tags">The tags to relate to this member.</param>
        /// <param name="ttl">Set the current expiration timespan to the whole key (not only this set). NULL to keep the current expiration.</param>
        Task AddToSetAsync<T>(string key, T value, string[] tags = null, TimeSpan? ttl = null);
        /// <summary>
        /// Adds the given value to a redis sorted set with the given score.
        /// (The latest expiration applies to the whole key)
        /// </summary>
        /// <typeparam name="T">The member type</typeparam>
        /// <param name="key">The redis set key.</param>
        /// <param name="score">The member score to store</param>
        /// <param name="value">The member value to store</param>
        /// <param name="tags">The tags to relate to this member.</param>
        /// <param name="ttl">Set the current expiration timespan to the whole key (not only this set). NULL to keep the current expiration.</param>
        Task AddToSortedSetAsync<T>(string key, double score, T value, string[] tags = null, TimeSpan? ttl = null);
        /// <summary>
        /// Removes the given value from a redis sorted set.
        /// Returns true if the value was removed. (false if the element does not exists in the set)
        /// </summary>
        /// <typeparam name="T">The member type</typeparam>
        /// <param name="key">The redis set key.</param>
        /// <param name="value">The member value to remove</param>
        Task<bool> RemoveFromSortedSetAsync<T>(string key, T value);
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
        Task<T> FetchHashedAsync<T>(string key, string field, Func<Task<T>> func, TimeSpan? expiry = null);
        /// <summary>
        /// Fetches hashed data from the cache, using the given cache key and field, and associates the field to the given tags.
        /// If there is data in the cache with the given key, then that data is returned, and the last three parameters are ignored.
        /// If there is no such data in the cache (a cache miss occurred), then the value returned by func will be
        /// written to the cache under the given cache key-field, and that will be returned.
        /// </summary>
        /// <param name="key">The cache key.</param>
        /// <param name="field">The field to obtain.</param>
        /// <param name="func">The function that returns the cache value, only executed when there is a cache miss.</param>
        /// <param name="tags">The tags to relate to this field.</param>
        /// <param name="expiry">The expiration timespan.</param>
        Task<T> FetchHashedAsync<T>(string key, string field, Func<Task<T>> func, string[] tags, TimeSpan? expiry = null);
        /// <summary>
        /// Fetches hashed data from the cache, using the given cache key and field, and associates the field to the tags returned by the given tag builder.
        /// If there is data in the cache with the given key, then that data is returned, and the last three parameters are ignored.
        /// If there is no such data in the cache (a cache miss occurred), then the value returned by func will be
        /// written to the cache under the given cache key-field, and that will be returned.
        /// </summary>
        /// <param name="key">The cache key.</param>
        /// <param name="field">The field to obtain.</param>
        /// <param name="func">The function that returns the cache value, only executed when there is a cache miss.</param>
        /// <param name="tagsBuilder">The tags builder to specify tags depending on the value.</param>
        /// <param name="expiry">The expiration timespan.</param>
        Task<T> FetchHashedAsync<T>(string key, string field, Func<Task<T>> func, Func<T, string[]> tagsBuilder, TimeSpan? expiry = null);
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
        Task<T> FetchObjectAsync<T>(string key, Func<Task<T>> func, TimeSpan? expiry = null);
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
        Task<T> FetchObjectAsync<T>(string key, Func<Task<T>> func, string[] tags, TimeSpan? expiry = null);
        /// <summary>
        /// Fetches data from the cache, using the given cache key.
        /// If there is data in the cache with the given key, then that data is returned.
        /// If there is no such data in the cache (a cache miss occurred), then the value returned by func will be
        /// written to the cache under the given cache key and associated to the given tags, and that will be returned.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key">The cache key.</param>
        /// <param name="func">The function that returns the cache value, only executed when there is a cache miss.</param>
        /// <param name="tagsBuilder">The tag builder to associte tags depending on the value.</param>
        /// <param name="expiry">The expiration timespan.</param>
        Task<T> FetchObjectAsync<T>(string key, Func<Task<T>> func, Func<T, string[]> tagsBuilder, TimeSpan? expiry = null);
        /// <summary>
        /// Set the value of a key
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <param name="ttl">The expiration.</param>
        /// <param name="when">Indicates when this operation should be performed.</param>
        Task SetObjectAsync<T>(string key, T value, TimeSpan? ttl = null, When when = When.Always);
        /// <summary>
        /// Set the value of a key, associating the key with the given tag(s).
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <param name="tags">The tags.</param>
        /// <param name="ttl">The time to live.</param>
        /// <param name="when">Indicates when this operation should be performed.</param>
        Task SetObjectAsync<T>(string key, T value, string[] tags, TimeSpan? ttl = null, When when = When.Always);
        /// <summary>
        /// Atomically sets key to value and returns the old value stored at key. 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key">The key.</param>
        /// <param name="value">The new value value.</param>
        /// <returns>The old value</returns>
        Task<T> GetSetObjectAsync<T>(string key, T value);
        /// <summary>
        /// Renames a tag related to a key.
        /// If the current tag is not related to the key, no operation is performed.
        /// If the current tag is related to the key, the tag relation is removed and the new tag relation is inserted.
        /// </summary>
        /// <param name="key">The key related to the tag.</param>
        /// <param name="currentTag">The current tag.</param>
        /// <param name="newTag">The new tag.</param>
        Task RenameTagForKeyAsync(string key, string currentTag, string newTag);
        /// <summary>
        /// Renames a tag related to a hash field.
        /// If the current tag is not related to the hash field, no operation is performed.
        /// If the current tag is related to the hash field, the tag relation is removed and the new tag relation is inserted.
        /// </summary>
        /// <param name="key">The hash key.</param>
        /// <param name="field">The hash field related to the tag.</param>
        /// <param name="currentTag">The current tag.</param>
        /// <param name="newTag">The new tag.</param>
        Task RenameTagForHashFieldAsync(string key, string field, string currentTag, string newTag);
        /// <summary>
        /// Renames a tag related to a set member.
        /// If the current tag is not related to the set member, no operation is performed.
        /// If the current tag is related to the set member, the tag relation is removed and the new tag relation is inserted.
        /// </summary>
        /// <param name="key">The set key.</param>
        /// <param name="member">The set member related to the current tag.</param>
        /// <param name="currentTag">The current tag.</param>
        /// <param name="newTag">The new tag.</param>
        Task RenameTagForSetMemberAsync<T>(string key, T member, string currentTag, string newTag);
        /// <summary>
        /// Get the value of a key
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key">The key.</param>
        /// <returns>``0.</returns>
        Task<T> GetObjectAsync<T>(string key);
        /// <summary>
        /// Gets all the keys related to the given tag(s).
        /// Returns a hashset with the keys.
        /// </summary>
        /// <param name="tags">The tags.</param>
        /// <param name="cleanUp">True to return only the existing keys within the tags (slower). Default is false.</param>
        /// <returns>HashSet{System.String}.</returns>
        Task<IEnumerable<string>> GetKeysByTagAsync(string[] tags, bool cleanUp = false);
        /// <summary>
        /// Removes all the keys related to the given tag(s).
        /// </summary>
        /// <param name="tags">The tags.</param>
        Task InvalidateKeysByTagAsync(params string[] tags);
        /// <summary>
        /// Determines if a key exists.
        /// </summary>
        /// <param name="key">The key.</param>
        Task<bool> KeyExistsAsync(string key);
        /// <summary>
        /// Sets the expiration of a key from a local date time expiration value.
        /// </summary>
        /// <param name="key">The key to expire</param>
        /// <param name="expiration">The expiration local date time</param>
        /// <returns>True is the key expiration was updated</returns>
        Task<bool> KeyExpireAsync(string key, DateTime expiration);
        /// <summary>
        /// Sets the time-to-live of a key from a timespan value.
        /// </summary>
        /// <param name="key">The key to expire</param>
        /// <param name="ttl">The TTL timespan</param>
        /// <returns>True if the key expiration was updated</returns>
        Task<bool> KeyTimeToLiveAsync(string key, TimeSpan ttl);
        /// <summary>
        /// Gets the time-to-live of a key.
        /// Returns NULL when key does not exist or does not have a timeout.
        /// </summary>
        /// <param name="key">The redis key to get its time-to-live</param>
        Task<TimeSpan?> KeyTimeToLiveAsync(string key);
        /// <summary>
        /// Removes the expiration of the given key.
        /// </summary>
        /// <param name="key">The key to persist</param>
        /// <returns>True is the key expiration was removed</returns>
        Task<bool> KeyPersistAsync(string key);
        /// <summary>
        /// Removes the specified key-value.
        /// </summary>
        /// <param name="key">The key.</param>
        Task<bool> RemoveAsync(string key);
        /// <summary>
        /// Removes the specified keys.
        /// </summary>
        /// <param name="keys">The keys to remove.</param>
        Task RemoveAsync(string[] keys);
        /// <summary>
        /// Gets a specified hased value from a key
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key">The key.</param>
        /// <param name="field">The field.</param>
        /// <returns>``0.</returns>
        Task<T> GetHashedAsync<T>(string key, string field);
        /// <summary>
        /// Gets a specified hased value from a key
        /// </summary>
        /// <typeparam name="TK">The type of the hash fields</typeparam>
        /// <typeparam name="TV">The type of the hash values</typeparam>
        /// <param name="key">The key.</param>
        /// <param name="field">The field.</param>
        Task<TV> GetHashedAsync<TK, TV>(string key, TK field);
        /// <summary>
        /// Removes a specified hased value from cache
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="field">The field.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        Task<bool> RemoveHashedAsync(string key, string field);
        /// <summary>
        /// Gets all the values from a hash.
        /// The keys of the dictionary are the field names and the values are the objects
        /// </summary>
        /// <param name="key">The key.</param>
        Task<IDictionary<string, T>> GetHashedAllAsync<T>(string key);
        /// <summary>
        /// Flushes all the databases on every master node.
        /// </summary>
        Task FlushAllAsync();
        /// <summary>
        /// Adds all the element arguments to the HyperLogLog data structure stored at the specified key.
        /// </summary>
        /// <typeparam name="T">The items type</typeparam>
        /// <param name="key">The redis key.</param>
        /// <param name="items">The items to add.</param>
        /// <returns><c>true</c> if at least 1 HyperLogLog internal register was altered, <c>false</c> otherwise.</returns>
        Task<bool> HyperLogLogAddAsync<T>(string key, T[] items);
        /// <summary>
        /// Adds the element to the HyperLogLog data structure stored at the specified key.
        /// </summary>
        /// <typeparam name="T">The items type</typeparam>
        /// <param name="key">The redis key.</param>
        /// <param name="item">The item to add.</param>
        /// <returns><c>true</c> if at least 1 HyperLogLog internal register was altered, <c>false</c> otherwise.</returns>
        Task<bool> HyperLogLogAddAsync<T>(string key, T item);
        /// <summary>
        /// Returns the approximated cardinality computed by the HyperLogLog data structure stored at the specified key, which is 0 if the variable does not exist.
        /// </summary>
        /// <param name="key">The redis key.</param>
        Task<long> HyperLogLogCountAsync(string key);
        /// <summary>
        /// Sets the specified key/values pairs to a hashset.
        /// (The latest expiration applies to the whole key)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key">The key.</param>
        /// <param name="fieldValues">The field keys and values to store</param>
        Task SetHashedAsync<T>(string key, IDictionary<string, T> fieldValues);
        /// <summary>
        /// Sets the specified value to a hashset using the pair hashKey+field.
        /// (The latest expiration applies to the whole key)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key">The key.</param>
        /// <param name="field">The field key</param>
        /// <param name="value">The value to store</param>
        /// <param name="ttl">Set the current expiration timespan to the whole key (not only this hash). NULL to keep the current expiration.</param>
        /// <param name="when">Indicates when this operation should be performed.</param>
        Task SetHashedAsync<T>(string key, string field, T value, TimeSpan? ttl = null, Contracts.When when = Contracts.When.Always);
        /// <summary>
        /// Sets the specified value to a hashset using the pair hashKey+field.
        /// (The latest expiration applies to the whole key)
        /// </summary>
        /// <typeparam name="TK">The type of the key field</typeparam>
        /// <typeparam name="TV">The value type</typeparam>
        /// <param name="key">The key.</param>
        /// <param name="field">The field key</param>
        /// <param name="value">The value to store</param>
        /// <param name="ttl">Set the current expiration timespan to the whole key (not only this hash). NULL to keep the current expiration.</param>
        /// <param name="when">Indicates when this operation should be performed.</param>
        Task SetHashedAsync<TK, TV>(string key, TK field, TV value, TimeSpan? ttl = null, Contracts.When when = Contracts.When.Always);
        /// <summary>
        /// Sets the specified value to a hashset using the pair hashKey+field.
        /// (The latest expiration applies to the whole key)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key">The key.</param>
        /// <param name="field">The field key</param>
        /// <param name="value">The value to store</param>
        /// <param name="tags">The tags to relate to this field.</param>
        /// <param name="ttl">Set the current expiration timespan to the whole key (not only this hash). NULL to keep the current expiration.</param>
        /// <param name="when">Indicates when this operation should be performed.</param>
        Task SetHashedAsync<T>(string key, string field, T value, string[] tags, TimeSpan? ttl = null, Contracts.When when = Contracts.When.Always);
        /// <summary>
        /// Sets the specified value to a hashset using the pair hashKey+field.
        /// (The latest expiration applies to the whole key)
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="field">The field key</param>
        /// <param name="value">The value to store</param>
        /// <param name="tags">The tags to relate to this field.</param>
        /// <param name="ttl">Set the current expiration timespan to the whole key (not only this hash). NULL to keep the current expiration.</param>
        /// <param name="when">Indicates when this operation should be performed.</param>
        Task SetHashedAsync<TK, TV>(string key, TK field, TV value, string[] tags, TimeSpan? ttl = null, Contracts.When when = Contracts.When.Always);
        /// <summary>
        /// Determines if a redis string key is included in any of the given tags.
        /// </summary>
        /// <param name="key">The redis string key to find</param>
        /// <param name="tags">The tags to look into</param>
        Task<bool> IsStringKeyInTagAsync(string key, params string[] tags);
        /// <summary>
        /// Determines if a redis hash field is included in any of the given tags.
        /// </summary>
        /// <param name="key">The redis hash key to find</param>
        /// <param name="field">The redis hash field to find</param>
        /// <param name="tags">The tags to look into</param>
        Task<bool> IsHashFieldInTagAsync<T>(string key, T field, params string[] tags);
        /// <summary>
        /// Determines if a redis set (or sorted set) member is included in any of the given tags.
        /// </summary>
        /// <param name="key">The redis set or sorted set key to find</param>
        /// <param name="member">The redis set member to find</param>
        /// <param name="tags">The tags to look into</param>
        Task<bool> IsSetMemberInTagAsync<T>(string key, T member, params string[] tags);
    }
}