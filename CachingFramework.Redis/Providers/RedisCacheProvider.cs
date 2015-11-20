using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using CachingFramework.Redis.Contracts.Providers;
using StackExchange.Redis;

namespace CachingFramework.Redis.Providers
{
    /// <summary>
    /// Cache provider implementation using Redis.
    /// </summary>
    internal class RedisCacheProvider : RedisProviderBase, ICacheProvider
    {
        #region Constructor
        /// <summary>
        /// Initializes a new instance of the <see cref="RedisCacheProvider"/> class.
        /// </summary>
        /// <param name="context">The context.</param>
        public RedisCacheProvider(RedisProviderContext context)
            :base(context)
        {
        }
        #endregion

        #region Fields
        /// <summary>
        /// The tag format for the keys representing tags
        /// </summary>
        protected const string TagFormat = ":$_tag_$:{0}";
        #endregion

        #region ICacheProvider Implementation
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
        public T FetchObject<T>(string key, Func<T> func, TimeSpan? expiry = null)
        {
            return FetchObject(key, func, null, expiry);
        }
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
        public T FetchObject<T>(string key, Func<T> func, string[] tags, TimeSpan? expiry = null)
        {
            T value;
            if (!TryGetObject(key, out value))
            {
                value = func();
                SetObject(key, value, tags, expiry);
            }
            return value;
        }
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
        public T FetchHashed<T>(string key, string field, Func<T> func, TimeSpan? expiry = null)
        {
            T value;
            if (!TryGetHashed(key, field, out value))
            {
                value = func();
                SetHashed(key, field, value, expiry);
            }
            return value;
        }
        /// <summary>
        /// Set the value of a key
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <param name="ttl">The expiration.</param>
        public void SetObject<T>(string key, T value, TimeSpan? ttl = null)
        {
            var serialized = Serializer.Serialize(value);
            RedisConnection.GetDatabase().StringSet(key, serialized, ttl);
        }
        /// <summary>
        /// Set the value of a key, associating the key with the given tag(s).
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <param name="tags">The tags.</param>
        /// <param name="ttl">The expiry.</param>
        public void SetObject<T>(string key, T value, string[] tags, TimeSpan? ttl = null)
        {
            if (tags == null || tags.Length == 0)
            {
                SetObject(key, value, ttl);
                return;
            }
            var serialized = Serializer.Serialize(value);
            var db = RedisConnection.GetDatabase();
            var batch = db.CreateBatch();
            foreach (var tagName in tags)
            {
                var tag = FormatTag(tagName);
                var expiration = GetExpiration(db, tag, ttl);
                // Add the tag-key relation
                batch.SetAddAsync(tag, key);
                // Set the expiration
                if (expiration != null)
                {
                    if (expiration == TimeSpan.MaxValue)
                    {
                        batch.KeyPersistAsync(tag);
                    }
                    else
                    {
                        batch.KeyExpireAsync(tag, expiration);
                    }
                }
            }
            // Add the key-value
            batch.StringSetAsync(key, serialized, ttl);
            batch.Execute();
        }
        /// <summary>
        /// Atomically sets key to value and returns the old value stored at key. 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key">The key.</param>
        /// <param name="value">The new value value.</param>
        /// <returns>The old value</returns>
        public T GetSetObject<T>(string key, T value)
        {
            var serialized = Serializer.Serialize(value);
            var oldValue = RedisConnection.GetDatabase().StringGetSet(key, serialized);
            if (oldValue.HasValue)
            {
                return Serializer.Deserialize<T>(oldValue);
            }
            return default(T);
        }
        /// <summary>
        /// Relates the given tags to a key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="tags">The tag(s).</param>
        public void AddTagsToKey(string key, string[] tags)
        {
            var db = RedisConnection.GetDatabase();
            var batch = db.CreateBatch();
            foreach (var tag in tags)
            {
                batch.SetAddAsync(FormatTag(tag), key);
            }
            batch.Execute();
        }
        /// <summary>
        /// Removes the relation between the given tags and a key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="tags">The tag(s).</param>
        public void RemoveTagsFromKey(string key, string[] tags)
        {
            var db = RedisConnection.GetDatabase();
            var batch = db.CreateBatch();
            foreach (var tagName in tags)
            {
                var tag = FormatTag(tagName);
                batch.SetRemoveAsync(tag, key);
            }
            batch.Execute();
        }
        /// <summary>
        /// Removes all the keys related to the given tag(s).
        /// </summary>
        /// <param name="tags">The tags.</param>
        public void InvalidateKeysByTag(params string[] tags)
        {
            var db = RedisConnection.GetDatabase();
            var keys = GetKeysByAllTagsNoCleanup(db, tags);
            var batch = db.CreateBatch();
            // Delete the keys
            foreach (var key in keys)
            {
                batch.KeyDeleteAsync(key);
            }
            // Delete the tags
            foreach (var tagName in tags)
            {
                batch.KeyDeleteAsync(FormatTag(tagName));
            }
            batch.Execute();
        }
        /// <summary>
        /// Gets all the keys related to the given tag(s).
        /// Returns a hashset with the keys.
        /// Also does the cleanup for the given tags if the parameter cleanUp is true.
        /// Since it is cluster compatible, and cluster does not allow multi-key operations, we cannot use SUNION or LUA scripts.
        /// </summary>
        /// <param name="tags">The tags.</param>
        /// <param name="cleanUp">True to return only the existing keys within the tags (slower). Default is false.</param>
        /// <returns>HashSet{System.String}.</returns>
        public ISet<string> GetKeysByTag(string[] tags, bool cleanUp = false)
        {
            var db = RedisConnection.GetDatabase();
            if (cleanUp)
            {
                return GetKeysByAllTagsWithCleanup(db, tags);
            }
            return GetKeysByAllTagsNoCleanup(db, tags);
        }
        /// <summary>
        /// Returns all the objects that has the given tag(s) related.
        /// Assumes all the objects are of the same type <typeparamref name="T" />.
        /// </summary>
        /// <typeparam name="T">The objects types</typeparam>
        /// <param name="tags">The tags</param>
        /// <returns>IEnumerable{``0}.</returns>
        public IEnumerable<T> GetObjectsByTag<T>(params string[] tags)
        {
            var db = RedisConnection.GetDatabase();
            ISet<string> keys = GetKeysByAllTagsNoCleanup(db, tags);
            foreach (var key in keys)
            {
                var value = db.StringGet(key);
                if (value.HasValue)
                {
                    yield return Serializer.Deserialize<T>(value);    
                }
            }
        }
        /// <summary>
        /// Gets a deserialized value from a key
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key">The key.</param>
        /// <returns>``0.</returns>
        public T GetObject<T>(string key)
        {
            var cacheValue = RedisConnection.GetDatabase().StringGet(key);
            if (cacheValue.HasValue)
            {
                return Serializer.Deserialize<T>(cacheValue);
            }
            return default(T);
        }
        /// <summary>
        /// Try to get the value of a key
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value"> When this method returns, contains the value associated with the specified key, if the key is found; 
        /// otherwise, the default value for the type of the value parameter.</param>
        /// <returns>True if the cache contains an element with the specified key; otherwise, false.</returns>
        public bool TryGetObject<T>(string key, out T value)
        {
            var cacheValue = RedisConnection.GetDatabase().StringGet(key);
            if (!cacheValue.HasValue)
            {
                value = default(T);
                return false;
            }
            value = Serializer.Deserialize<T>(cacheValue);
            return true;
        }
        /// <summary>
        /// Returns the entire collection of tags
        /// </summary>
        public ISet<string> GetAllTags()
        {
            var tags = new List<RedisKey>();
            RunInAllMasters(svr => tags.AddRange(svr.Keys(0, string.Format(TagFormat, "*"))));
            int startIndex = string.Format(TagFormat, "").Length;
            return new HashSet<string>(tags.Select(rv => rv.ToString().Substring(startIndex)));
        }
        /// <summary>
        /// Removes the specified key-value.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        /// <remarks>Redis command: DEL key</remarks>
        public bool Remove(string key)
        {
            return RedisConnection.GetDatabase().KeyDelete(key);
        }
        /// <summary>
        /// Determines if a key exists.
        /// </summary>
        /// <param name="key">The key.</param>
        public bool KeyExists(string key)
        {
            return RedisConnection.GetDatabase().KeyExists(key);
        }
        /// <summary>
        /// Sets the expiration of a key from a local date time expiration value.
        /// </summary>
        /// <param name="key">The key to expire</param>
        /// <param name="expiration">The expiration local date time</param>
        /// <returns>True is the key expiration was updated</returns>
        public bool KeyExpire(string key, DateTime expiration)
        {
            return RedisConnection.GetDatabase().KeyExpire(key, expiration);

        }
        /// <summary>
        /// Sets the time-to-live of a key from a timespan value.
        /// </summary>
        /// <param name="key">The key to expire</param>
        /// <param name="ttl">The TTL timespan</param>
        /// <returns>True is the key expiration was updated</returns>
        public bool KeyTimeToLive(string key, TimeSpan ttl)
        {
            return RedisConnection.GetDatabase().KeyExpire(key, ttl);
        }
        /// <summary>
        /// Removes the expiration of the given key.
        /// </summary>
        /// <param name="key">The key to persist</param>
        /// <returns>True is the key expiration was removed</returns>
        public bool KeyPersist(string key)
        {
            return RedisConnection.GetDatabase().KeyPersist(key);
        }
        /// <summary>
        /// Removes the specified keys.
        /// </summary>
        /// <param name="keys">The keys to remove.</param>
        public void Remove(string[] keys)
        {
            var batch = RedisConnection.GetDatabase().CreateBatch();
            foreach (var key in keys)
            {
                batch.KeyDeleteAsync(key);
            }
            batch.Execute();
        }
        /// <summary>
        /// Sets the specified value to a hashset using the pair hashKey+field.
        /// (The latest expiration applies to the whole key)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key">The key.</param>
        /// <param name="field">The field key</param>
        /// <param name="value">The value to store</param>
        /// <param name="ttl">Set the current expiration timespan to the whole key (not only this hash). NULL to keep the current expiration.</param>
        public void SetHashed<T>(string key, string field, T value, TimeSpan? ttl = null)
        {
            var db = RedisConnection.GetDatabase();
            var batch = db.CreateBatch();
            batch.HashSetAsync(key, field, Serializer.Serialize(value));
            var expiration = GetExpiration(db, key, ttl);
            if (expiration != null)
            {
                if (expiration == TimeSpan.MaxValue)
                {
                    batch.KeyPersistAsync(key);
                }
                else
                {
                    batch.KeyExpireAsync(key, expiration);
                }
            }
            batch.Execute();
        }
        /// <summary>
        /// Sets the specified key/values pairs to a hashset.
        /// (The latest expiration applies to the whole key)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key">The key.</param>
        /// <param name="fieldValues">The field keys and values to store</param>
        /// <param name="ttl">Set the current expiration timespan to the whole key (not only this hash). NULL to keep the current expiration.</param>
        public void SetHashed<T>(string key, IDictionary<string, T> fieldValues, TimeSpan? ttl = null)
        {
            var db = RedisConnection.GetDatabase();
            db.HashSet(key, fieldValues.Select(x => new HashEntry(x.Key, Serializer.Serialize(x.Value))).ToArray());
        }
        /// <summary>
        /// Gets a specified hashed value from a key
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key">The key.</param>
        /// <param name="field">The field.</param>
        public T GetHashed<T>(string key, string field)
        {
            var cacheValue = RedisConnection.GetDatabase().HashGet(key, field);
            return cacheValue.HasValue ? Serializer.Deserialize<T>(cacheValue) : default(T);
        }
        /// <summary>
        /// Try to get the value of an element in a hashed key
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="field">The hash field.</param>
        /// <param name="value">When this method returns, contains the value associated with the specified hash field within the key, if the key and field are found; 
        /// otherwise, the default value for the type of the value parameter.</param>
        /// <returns>True if the cache contains a hashed element with the specified key and field; otherwise, false.</returns>
        public bool TryGetHashed<T>(string key, string field, out T value)
        {
            var cacheValue = RedisConnection.GetDatabase().HashGet(key, field);
            if (!cacheValue.HasValue)
            {
                value = default(T);
                return false;
            }
            value = Serializer.Deserialize<T>(cacheValue);
            return true;
        }
        /// <summary>
        /// Removes a specified hased value from cache
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="field">The field.</param>
        public bool RemoveHashed(string key, string field)
        {
            return RedisConnection.GetDatabase().HashDelete(key, field);
        }
        /// <summary>
        /// Gets all the values from a hash, assuming all the values in the hash are of the same type <typeparamref name="T" />.
        /// The keys of the dictionary are the field names and the values are the objects
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key">The key.</param>
        public IDictionary<string, T> GetHashedAll<T>(string key)
        {
            return RedisConnection.GetDatabase()
                .HashGetAll(key)
                .ToDictionary(k => k.Name.ToString(), v => Serializer.Deserialize<T>(v.Value));
        }
        /// <summary>
        /// Adds all the element arguments to the HyperLogLog data structure stored at the specified key.
        /// </summary>
        /// <typeparam name="T">The items type</typeparam>
        /// <param name="key">The redis key.</param>
        /// <param name="items">The items to add.</param>
        /// <returns><c>true</c> if at least 1 HyperLogLog internal register was altered, <c>false</c> otherwise.</returns>
        public bool HyperLogLogAdd<T>(string key, T[] items)
        {
            return RedisConnection.GetDatabase()
                    .HyperLogLogAdd(key, items.Select(x => (RedisValue) Serializer.Serialize(x)).ToArray());
        }
        /// <summary>
        /// Adds the element to the HyperLogLog data structure stored at the specified key.
        /// </summary>
        /// <typeparam name="T">The items type</typeparam>
        /// <param name="key">The redis key.</param>
        /// <param name="item">The item to add.</param>
        /// <returns><c>true</c> if at least 1 HyperLogLog internal register was altered, <c>false</c> otherwise.</returns>
        public bool HyperLogLogAdd<T>(string key, T item)
        {
            return HyperLogLogAdd(key, new[] { item });
        }
        /// <summary>
        /// Returns the approximated cardinality computed by the HyperLogLog data structure stored at the specified key, which is 0 if the variable does not exist.
        /// </summary>
        /// <param name="key">The redis key.</param>
        /// <returns>System.Int64.</returns>
        public long HyperLogLogCount(string key)
        {
            return RedisConnection.GetDatabase()
                    .HyperLogLogLength(key);
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Returns the maximum TTL between the current key TTL and the given TTL
        /// </summary>
        /// <param name="db">The database.</param>
        /// <param name="key">The key.</param>
        /// <param name="ttl">The TTL.</param>
        private static TimeSpan? GetExpiration(IDatabase db, string key, TimeSpan? ttl)
        {
            bool preexistent = db.KeyExists(key);
            TimeSpan? curr = preexistent ? db.KeyTimeToLive(key) : null;
            if (ttl != null && curr != null)
            {
                // We have an expiration on both keys, use the max for the key
                return curr > ttl ? curr : ttl;
            }
            if (preexistent && ttl == null)
            {
                //Key is preexistent and no expiration given
                return TimeSpan.MaxValue;
            }
            return ttl;
        }
        /// <summary>
        /// Get all the keys related to a tag(s), the keys returned are not tested for existence.
        /// </summary>
        /// <param name="db">The database.</param>
        /// <param name="tags">The tags.</param>
        private static ISet<string> GetKeysByAllTagsNoCleanup(IDatabase db, params string[] tags)
        {
            var keys = new List<string>();
            foreach (var tagName in tags)
            {
                var tag = FormatTag(tagName);
                if (db.KeyType(tag) == RedisType.Set)
                {
                    keys.AddRange(db.SetMembers(tag).Select(rv => rv.ToString()));
                }
            }
            return new HashSet<string>(keys);
        }
        /// <summary>
        /// Get all the keys related to a tag(s), only returns the keys that currently exists.
        /// </summary>
        /// <param name="db">The database.</param>
        /// <param name="tags">The tags.</param>
        private static ISet<string> GetKeysByAllTagsWithCleanup(IDatabase db, params string[] tags)
        {
            var ret = new HashSet<string>();
            var toRemove = new List<RedisValue>();
            foreach (var tagName in tags)
            {
                var tag = FormatTag(tagName);
                if (db.KeyType(tag) == RedisType.Set)
                {
                    var tagKeys = db.SetMembers(tag);
                    //Get the existing keys and delete the dead keys
                    foreach (var key in tagKeys)
                    {
                        if (db.KeyExists(key.ToString()))
                        {
                            ret.Add(key);
                        }
                        else
                        {
                            toRemove.Add(key);
                        }
                    }
                    if (toRemove.Count > 0)
                    {
                        db.SetRemove(tag, toRemove.ToArray());
                    }
                }
            }
            return ret;
        }
        /// <summary>
        /// Return the RedisKey used for a tag
        /// </summary>
        /// <param name="tag">The tag name</param>
        /// <returns>RedisKey.</returns>
        private static RedisKey FormatTag(string tag)
        {
            return string.Format(TagFormat, tag);
        }
        /// <summary>
        /// Runs a Server command in all the master servers.
        /// </summary>
        /// <param name="action">The action.</param>
        private void RunInAllMasters(Action<IServer> action)
        {
            var masters = new List<EndPoint>();
            foreach (var ep in RedisConnection.GetEndPoints())
            {
                var server = RedisConnection.GetServer(ep);
                if (server.IsConnected)
                {
                    if (server.ServerType == ServerType.Cluster)
                    {
                        masters.AddRange(server.ClusterConfiguration.Nodes.Where(n => !n.IsSlave).Select(n => n.EndPoint));
                        break;
                    }
                    if (server.ServerType == ServerType.Standalone && !server.IsSlave)
                    {
                        masters.Add(ep);
                        break;
                    }
                }
            }
            foreach (var ep in masters)
            {
                action(RedisConnection.GetServer(ep));
            }
        }
        /// <summary>
        /// Flushes all the databases on every master node.
        /// </summary>
        public void FlushAll()
        {
            RunInAllMasters(svr => svr.FlushAllDatabases());
        }
        #endregion
    }
}
