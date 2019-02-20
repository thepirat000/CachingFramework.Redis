using System;
using System.Collections.Generic;

namespace CachingFramework.Redis.Contracts.Providers
{
    /// <summary>
    /// Cache Provider internal contract
    /// </summary>
    public interface ICacheProvider : ICacheProviderAsync
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
        T FetchHashed<T>(string key, string field, Func<T> func, TimeSpan? expiry = null);
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
        T FetchHashed<T>(string key, string field, Func<T> func, string[] tags, TimeSpan? expiry = null);
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
        T FetchHashed<T>(string key, string field, Func<T> func, Func<T, string[]> tagsBuilder, TimeSpan? expiry = null);
        /// <summary>
        /// Fetches hashed data from the cache, using the given cache key and field, and associates the field to the tags returned by the given tag builder.
        /// If there is data in the cache with the given key, then that data is returned, and the last three parameters are ignored.
        /// If there is no such data in the cache (a cache miss occurred), then the value returned by func will be
        /// written to the cache under the given cache key-field, and that will be returned.
        /// </summary>
        /// <typeparam name="TK">The field type</typeparam>
        /// <typeparam name="TV">The value type</typeparam>
        /// <param name="key">The cache key.</param>
        /// <param name="field">The field to obtain.</param>
        /// <param name="func">The function that returns the cache value, only executed when there is a cache miss.</param>
        /// <param name="tagsBuilder">The tags builder to specify tags depending on the value.</param>
        /// <param name="expiry">The expiration timespan.</param>
        TV FetchHashed<TK, TV>(string key, TK field, Func<TV> func, Func<TV, string[]> tagsBuilder, TimeSpan? expiry = null);
        /// <summary>
        /// Fetches hashed data from the cache, using the given cache key and field.
        /// If there is data in the cache with the given key, then that data is returned.
        /// If there is no such data in the cache (a cache miss occurred), then the value returned by func will be
        /// written to the cache under the given cache key-field, and that will be returned.
        /// </summary>
        /// <typeparam name="TK">The field type</typeparam>
        /// <typeparam name="TV">The value type</typeparam>
        /// <param name="key">The cache key.</param>
        /// <param name="field">The field to obtain.</param>
        /// <param name="func">The function that returns the cache value, only executed when there is a cache miss.</param>
        /// <param name="expiry">The expiration timespan.</param>
        TV FetchHashed<TK, TV>(string key, TK field, Func<TV> func, TimeSpan? expiry = null);
        /// <summary>
        /// Fetches hashed data from the cache, using the given cache key and field, and associates the field to the given tags.
        /// If there is data in the cache with the given key, then that data is returned, and the last three parameters are ignored.
        /// If there is no such data in the cache (a cache miss occurred), then the value returned by func will be
        /// written to the cache under the given cache key-field, and that will be returned.
        /// </summary>
        /// <typeparam name="TK">The field type</typeparam>
        /// <typeparam name="TV">The value type</typeparam>
        /// <param name="key">The cache key.</param>
        /// <param name="field">The field to obtain.</param>
        /// <param name="func">The function that returns the cache value, only executed when there is a cache miss.</param>
        /// <param name="tags">The tags to relate to this field.</param>
        /// <param name="expiry">The expiration timespan.</param>
        TV FetchHashed<TK, TV>(string key, TK field, Func<TV> func, string[] tags, TimeSpan? expiry = null);
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
        T FetchObject<T>(string key, Func<T> func, Func<T, string[]> tagsBuilder, TimeSpan? expiry = null);
        /// <summary>
        /// Set the value of a key
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <param name="ttl">The expiration.</param>
        /// <param name="when">Indicates when this operation should be performed.</param>
        void SetObject<T>(string key, T value, TimeSpan? ttl = null, When when = When.Always);
        /// <summary>
        /// Set the value of a key, associating the key with the given tag(s).
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <param name="tags">The tags.</param>
        /// <param name="ttl">The time to live.</param>
        /// <param name="when">Indicates when this operation should be performed.</param>
        void SetObject<T>(string key, T value, string[] tags, TimeSpan? ttl = null, When when = When.Always);
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
        /// Renames a tag related to a key.
        /// If the current tag is not related to the key, no operation is performed.
        /// If the current tag is related to the key, the tag relation is removed and the new tag relation is inserted.
        /// </summary>
        /// <param name="key">The key related to the tag.</param>
        /// <param name="currentTag">The current tag.</param>
        /// <param name="newTag">The new tag.</param>
        void RenameTagForKey(string key, string currentTag, string newTag);
        /// <summary>
        /// Relates the given tags to a field inside a hash key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="field">The field.</param>
        /// <param name="tags">The tag(s).</param>
        void AddTagsToHashField(string key, string field, string[] tags);
        /// <summary>
        /// Relates the given tags to a member inside a redis set, sorted set or geospatial index.
        /// </summary>
        /// <param name="key">The redis set, sorted set or geospatial index key.</param>
        /// <param name="member">The set member.</param>
        /// <param name="tags">The tag(s).</param>
        void AddTagsToSetMember<T>(string key, T member, string[] tags);
        /// <summary>
        /// Renames a tag related to a hash field.
        /// If the current tag is not related to the hash field, no operation is performed.
        /// If the current tag is related to the hash field, the tag relation is removed and the new tag relation is inserted.
        /// </summary>
        /// <param name="key">The hash key.</param>
        /// <param name="field">The hash field related to the tag.</param>
        /// <param name="currentTag">The current tag.</param>
        /// <param name="newTag">The new tag.</param>
        void RenameTagForHashField(string key, string field, string currentTag, string newTag);
        /// <summary>
        /// Renames a tag related to a set member.
        /// If the current tag is not related to the set member, no operation is performed.
        /// If the current tag is related to the set member, the tag relation is removed and the new tag relation is inserted.
        /// </summary>
        /// <param name="key">The set key.</param>
        /// <param name="member">The set member related to the current tag.</param>
        /// <param name="currentTag">The current tag.</param>
        /// <param name="newTag">The new tag.</param>
        void RenameTagForSetMember<T>(string key, T member, string currentTag, string newTag);
        /// <summary>
        /// Removes the relation between the given tags and a field in a hash.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="field">The field.</param>
        /// <param name="tags">The tag(s).</param>
        void RemoveTagsFromHashField(string key, string field, string[] tags);
        /// <summary>
        /// Removes the relation between the given tags and a key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="tags">The tag(s).</param>
        void RemoveTagsFromKey(string key, string[] tags);
        /// <summary>
        /// Removes the relation between the given tags and a set member.
        /// </summary>
        /// <param name="key">The set key.</param>
        /// <param name="member">The set member related to the tags.</param>
        /// <param name="tags">The tag(s).</param>
        void RemoveTagsFromSetMember<T>(string key, T member, string[] tags);
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
        IEnumerable<string> GetKeysByTag(string[] tags, bool cleanUp = false);
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
        IEnumerable<string> GetAllTags();
        /// <summary>
        /// Return the keys that matches a specified pattern.
        /// Will use SCAN or KEYS depending on the server capabilities.
        /// </summary>
        /// <param name="pattern">The glob-style pattern to match</param>
        IEnumerable<string> GetKeysByPattern(string pattern);
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
        /// <returns>True if the key expiration was updated</returns>
        bool KeyTimeToLive(string key, TimeSpan ttl);
        /// <summary>
        /// Gets the time-to-live of a key.
        /// Returns NULL when key does not exist or does not have a timeout.
        /// </summary>
        /// <param name="key">The redis key to get its time-to-live</param>
        TimeSpan? KeyTimeToLive(string key);
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
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key">The key.</param>
        /// <param name="field">The field key</param>
        /// <param name="value">The value to store</param>
        /// <param name="ttl">Set the current expiration timespan to the whole key (not only this field). NULL to keep the current expiration.</param>
        /// <param name="when">Indicates when this operation should be performed.</param>
        void SetHashed<T>(string key, string field, T value, TimeSpan? ttl = null, When when = When.Always);
        /// <summary>
        /// Sets the specified value to a hashset using the pair hashKey+field.
        /// The field can be any serializable type
        /// </summary>
        /// <typeparam name="TK">The field type</typeparam>
        /// <typeparam name="TV">The value type</typeparam>
        /// <param name="key">The key.</param>
        /// <param name="field">The field key</param>
        /// <param name="value">The value to store</param>
        /// <param name="ttl">Set the current expiration timespan to the whole key (not only this field). NULL to keep the current expiration.</param>
        /// <param name="when">Indicates when this operation should be performed.</param>
        void SetHashed<TK, TV>(string key, TK field, TV value, TimeSpan? ttl = null, When when = When.Always);
        
        
        /// <summary>
        /// Sets multiple values to the hashset stored on the given key.
        /// The field can be any serializable type
        /// </summary>
        /// <typeparam name="TK">The field type</typeparam>
        /// <typeparam name="TV">The value type</typeparam>
        /// <param name="key">The key.</param>
        /// <param name="fieldValues">The field keys and values</param>
        /// <param name="ttl">Set the current expiration timespan to the whole key (not only this field). NULL to keep the current expiration.</param>
        /// <param name="when">Indicates when this operation should be performed.</param>
        void SetHashed<TK, TV>(string key, IDictionary<TK, TV> fieldValues, TimeSpan? ttl = null, When when = When.Always);

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
        void SetHashed<T>(string key, string field, T value, string[] tags, TimeSpan? ttl = null, When when = When.Always);
        /// <summary>
        /// Sets the specified value to a hashset using the pair hashKey+field.
        /// The field can be any serializable type
        /// (The latest expiration applies to the whole key)
        /// </summary>
        /// <typeparam name="TK">The field type</typeparam>
        /// <typeparam name="TV">The value type</typeparam>
        /// <param name="key">The key.</param>
        /// <param name="field">The field key</param>
        /// <param name="value">The value to store</param>
        /// <param name="tags">The tags to relate to this field.</param>
        /// <param name="ttl">Set the current expiration timespan to the whole key (not only this hash). NULL to keep the current expiration.</param>
        /// <param name="when">Indicates when this operation should be performed.</param>
        void SetHashed<TK, TV>(string key, TK field, TV value, string[] tags, TimeSpan? ttl = null, Contracts.When when = Contracts.When.Always);
        /// <summary>
        /// Adds the given value to a redis set.
        /// (The latest expiration applies to the whole key)
        /// </summary>
        /// <typeparam name="T">The member type</typeparam>
        /// <param name="key">The redis set key.</param>
        /// <param name="value">The member value to store</param>
        /// <param name="tags">The tags to relate to this member.</param>
        /// <param name="ttl">Set the current expiration timespan to the whole key (not only this set). NULL to keep the current expiration.</param>
        void AddToSet<T>(string key, T value, string[] tags = null, TimeSpan? ttl = null);
        /// <summary>
        /// Removes the given value from a redis set.
        /// Returns true if the value was removed. (false if the element does not exists in the set)
        /// </summary>
        /// <typeparam name="T">The member type</typeparam>
        /// <param name="key">The redis set key.</param>
        /// <param name="value">The member value to remove</param>
        bool RemoveFromSet<T>(string key, T value);
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
        void AddToSortedSet<T>(string key, double score, T value, string[] tags = null, TimeSpan? ttl = null);
        /// <summary>
        /// Removes the given value from a redis sorted set.
        /// Returns true if the value was removed. (false if the element does not exists in the set)
        /// </summary>
        /// <typeparam name="T">The member type</typeparam>
        /// <param name="key">The redis set key.</param>
        /// <param name="value">The member value to remove</param>
        bool RemoveFromSortedSet<T>(string key, T value);
        /// <summary>
        /// Sets the specified key/values pairs to a hashset.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="fieldValues">The field keys and values to store</param>
        void SetHashed<T>(string key, IDictionary<string, T> fieldValues);
        /// <summary>
        /// Gets a specified hased value from a key
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key">The key.</param>
        /// <param name="field">The field.</param>
        /// <returns>``0.</returns>
        T GetHashed<T>(string key, string field);
        /// <summary>
        /// Gets a specified hased value from a key
        /// </summary>
        /// <typeparam name="TK">The type of the hash fields</typeparam>
        /// <typeparam name="TV">The type of the hash values</typeparam>
        /// <param name="key">The key.</param>
        /// <param name="field">The field.</param>
        TV GetHashed<TK, TV>(string key, TK field);
        /// <summary>
        /// Try to get the value of a hash key assuming the fields are strings
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="field">The hash field.</param>
        /// <param name="value"> When this method returns, contains the value associated with the specified hash field within the key, if the key and field are found; 
        /// otherwise, the default value for the type of the value parameter.</param>
        /// <returns>True if the cache contains a hashed element with the specified key and field; otherwise, false.</returns>
        bool TryGetHashed<T>(string key, string field, out T value);
        /// <summary>
        /// Try to get the value of a hash key
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="field">The hash field.</param>
        /// <param name="value">When this method returns, contains the value associated with the specified hash field within the key, if the key and field are found; 
        /// otherwise, the default value for the type of the value parameter.</param>
        /// <returns>True if the cache contains a hashed element with the specified key and field; otherwise, false.</returns>
        bool TryGetHashed<TK, TV>(string key, TK field, out TV value);
        /// <summary>
        /// Removes a specified hased value from cache assuming the field as a string
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="field">The field.</param>
        bool RemoveHashed(string key, string field);
        /// <summary>
        /// Removes a specified hased value from cache
        /// </summary>
        /// <typeparam name="TK">The type of the hash fields</typeparam>
        /// <param name="key">The key.</param>
        /// <param name="field">The field.</param>
        bool RemoveHashed<TK>(string key, TK field);
        /// <summary>
        /// Gets all the values from a hash, assuming all the values in the hash are of the same type <typeparamref name="T" />.
        /// The keys of the dictionary are the field names and the values are the objects.
        /// The fields are assumed to be strings, otherwise use the overload indicating the field type.
        /// </summary>
        /// <typeparam name="T">The value type</typeparam>
        /// <param name="key">The redis key.</param>
        IDictionary<string, T> GetHashedAll<T>(string key);
        /// <summary>
        /// Gets all the values from a hash, assuming all the values in the hash are of the same type <typeparamref name="TV" />.
        /// The keys of the dictionary are the field names of type <typeparamref name="TK" /> and the values are the objects of type <typeparamref name="TV" />.
        /// </summary>
        /// <typeparam name="TK">The field type</typeparam>
        /// <typeparam name="TV">The value type</typeparam>
        /// <param name="key">The redis key.</param>
        IDictionary<TK, TV> GetHashedAll<TK, TV>(string key);
        /// <summary>
        /// Matches a pattern on the field name of a hash, returning its values, assuming all the values in the hash are of the same type <typeparamref name="T" />.
        /// The keys of the dictionary are the field names and the values are the objects
        /// </summary>
        /// <typeparam name="T">The field value type</typeparam>
        /// <param name="key">The key.</param>
        /// <param name="pattern">The glob-style pattern to match.</param>
        /// <param name="pageSize">The scan page size to use.</param>
        IEnumerable<KeyValuePair<string, T>> ScanHashed<T>(string key, string pattern, int pageSize = 10);
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
        /// <summary>
        /// Determines if a redis string key is included in any of the given tags.
        /// </summary>
        /// <param name="key">The redis string key to find</param>
        /// <param name="tags">The tags to look into</param>
        bool IsStringKeyInTag(string key, params string[] tags);
        /// <summary>
        /// Determines if a redis hash field is included in any of the given tags.
        /// </summary>
        /// <param name="key">The redis hash key to find</param>
        /// <param name="field">The redis hash field to find</param>
        /// <param name="tags">The tags to look into</param>
        bool IsHashFieldInTag<T>(string key, T field, params string[] tags);
        /// <summary>
        /// Determines if a redis set (or sorted set) member is included in any of the given tags.
        /// </summary>
        /// <param name="key">The redis set or sorted set key to find</param>
        /// <param name="member">The redis set member to find</param>
        /// <param name="tags">The tags to look into</param>
        bool IsSetMemberInTag<T>(string key, T member, params string[] tags);
        /// <summary>
        /// Get all the members related to the given tag
        /// </summary>
        /// <param name="tag">The tag name to get its members</param>
        IEnumerable<TagMember> GetMembersByTag(string tag);
        /// <summary>
        /// Gets the specified hashed values from an array of hash fields
        /// </summary>
        /// <typeparam name="TK">The type of the hash fields</typeparam>
        /// <typeparam name="TV">The type of the hash values</typeparam>
        /// <param name="key">The key.</param>
        /// <param name="fields">The fields.</param>
        TV[] GetHashed<TK, TV>(string key, params TK[] fields);
        /// <summary>
        /// Gets the specified hashed values from an array of hash fields of type string
        /// </summary>
        /// <typeparam name="TV">The type of the hash values</typeparam>
        /// <param name="key">The key.</param>
        /// <param name="fields">The fields to get.</param>
        TV[] GetHashed<TV>(string key, params string[] fields);
    }
}
