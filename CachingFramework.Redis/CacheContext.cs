using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using CachingFramework.Redis.Contracts;
using CachingFramework.Redis.Providers;
using CachingFramework.Redis.RedisObjects;
using CachingFramework.Redis.Serializers;

namespace CachingFramework.Redis
{
    /// <summary>
    /// Cache Context containing the public API.
    /// </summary>
    public sealed class CacheContext
    {
        #region Fields
        /// <summary>
        /// The cache provider for this Cache instance
        /// </summary>
        private readonly ICacheProvider _cacheProvider;
        private readonly ISerializer _serializer;
        #endregion
        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="CacheContext" /> class using Redis in localhost server default port 6379, and using the default BinarySerializer.
        /// </summary>
        public CacheContext()
            : this("localhost:6379")
        {
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="CacheContext" /> class given the cache engine type and its configuration string, and using the default BinarySerializer.
        /// </summary>
        /// <param name="configuration">The configuration string.</param>
        public CacheContext(string configuration)
            : this(configuration, new BinarySerializer())
        {
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="CacheContext" /> class.
        /// </summary>
        /// <param name="configuration">The configuration string.</param>
        /// <param name="serializer">The serializer.</param>
        public CacheContext(string configuration, ISerializer serializer)
        {
            _serializer = serializer;
            _cacheProvider = new RedisCacheProvider(configuration, serializer);
        }
        #endregion
        #region Public methods
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
        public T FetchObject<T>(string key, Func<T> func, string[] tags = null, TimeSpan? expiry = null)
        {
            T value = GetObject<T>(key);
            if (null == value)
            {
                value = func();
                if (tags == null || tags.Length == 0)
                {
                    SetObject(key, value, expiry);
                }
                else
                {
                    SetObject(key, value, tags, expiry);
                }
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
            T value = GetHashed<T>(key, field);
            if (null == value)
            {
                value = func();
                SetHashed(key, field, value, expiry);
            }
            return value;
        }
        /// <summary>
        /// Adds an object to the cache
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key">The cache key.</param>
        /// <param name="value">The value.</param>
        /// <param name="expiry">The expiry.</param>
        public void SetObject<T>(string key, T value, TimeSpan? expiry = null)
        {
            _cacheProvider.SetObject(key, value, expiry);
        }
        /// <summary>
        /// Set the value of a key, associating the key with the given tag(s).
        /// If no tag is provided, it behaves as the Set operation.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <param name="tags">The tags.</param>
        /// <param name="expiry">The expiry.</param>
        public void SetObject<T>(string key, T value, string[] tags, TimeSpan? expiry = null)
        {
            if (tags == null || tags.Length == 0)
            {
                SetObject(key, value, expiry);
                return;
            }
            _cacheProvider.SetObject(key, value, tags, expiry);
        }
        /// <summary>
        /// Gets an object from cache
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key">The key.</param>
        /// <returns>``0.</returns>
        public T GetObject<T>(string key)
        {
            return _cacheProvider.GetObject<T>(key);
        }
        /// <summary>
        /// Gets the keys by all tags.
        /// </summary>
        /// <param name="tags">The tags.</param>
        /// <param name="cleanUp">True to return only the existing keys within the tags (slower). Default is false.</param>
        /// <returns>HashSet{System.String}.</returns>
        public ISet<string> GetKeysByTag(string[] tags, bool cleanUp = false)
        {
            return _cacheProvider.GetKeysByTag(tags, cleanUp);
        }
        /// <summary>
        /// Gets all the keys for a tag.
        /// </summary>
        /// <param name="tag">The tag.</param>
        /// <param name="cleanUp">True to return only the existing keys within the tags (slower). Default is false.</param>
        /// <returns>HashSet{System.String}.</returns>
        public ISet<string> GetKeysByTag(string tag, bool cleanUp = false)
        {
            return GetKeysByTag(new [] {tag}, cleanUp);
        }
        /// <summary>
        /// Returns all the objects that has the given tag(s) related.
        /// Assumes all the objects are of the same type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The objects types</typeparam>
        /// <param name="tags">The tags</param>
        public IEnumerable<T> GetObjectsByTag<T>(string[] tags)
        {
            return _cacheProvider.GetObjectsByTag<T>(tags);
        }
        /// <summary>
        /// Returns all the objects that has the given tag related.
        /// Assumes all the objects are of the same type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The objects types</typeparam>
        /// <param name="tag">The tag</param>
        public IEnumerable<T> GetObjectsByTag<T>(string tag)
        {
            return GetObjectsByTag<T>(new[] { tag });
        }
        /// <summary>
        /// Removes the keys by all tags.
        /// </summary>
        /// <param name="tags">The tags.</param>
        /// <returns>Returns the actual number of keys removed.</returns>
        public void InvalidateKeysByTag(params string[] tags)
        {
            _cacheProvider.InvalidateKeysByTag(tags);
        }
        /// <summary>
        /// Returns the entire collection of tags
        /// </summary>
        public ISet<string> GetAllTags()
        {
            return _cacheProvider.GetAllTags();
        }
        /// <summary>
        /// Removes the specified key-value.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>True if the key was removed</returns>
        public bool Remove(string key)
        {
            return _cacheProvider.Remove(key);
        }
        /// <summary>
        /// Relates the given tags to a key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="tags">The tag(s).</param>
        public void AddTagsToKey(string key, string[] tags)
        {
            _cacheProvider.AddTagsToKey(key, tags);
        }
        /// <summary>
        /// Removes the relation between the given tags and a key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="tags">The tag(s).</param>
        public void RemoveTagsFromKey(string key, string[] tags)
        {
            _cacheProvider.RemoveTagsFromKey(key, tags);
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
            _cacheProvider.SetHashed(key, field, value, ttl);
        }
        /// <summary>
        /// Sets the specified key/values pairs to a hashset.
        /// (The latest expiration applies to the whole key)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key">The key.</param>
        /// <param name="fieldValues">The field keys and values to store</param>
        /// <param name="ttl">Set the current expiration timespan to the whole key. NULL to keep the current expiration.</param>
        public void SetHashed<T>(string key, IDictionary<string, T> fieldValues, TimeSpan? ttl = null)
        {
            _cacheProvider.SetHashed<T>(key, fieldValues, ttl);
        }
        /// <summary>
        /// Gets a specified hased value from a key
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key">The redis key containing the hash.</param>
        /// <param name="field">The field to obtain.</param>
        public T GetHashed<T>(string key, string field)
        {
            return _cacheProvider.GetHashed<T>(key, field);
        }
        /// <summary>
        /// Gets all the values from a hash.
        /// The keys of the dictionary are the field names and the values are the objects
        /// </summary>
        /// <param name="key">The key.</param>
        public IDictionary<string, T> GetHashedAll<T>(string key)
        {
            return _cacheProvider.GetHashedAll<T>(key);
        }
        /// <summary>
        /// Removes a specified hased value from cache
        /// </summary>
        /// <param name="key">The redis key containing the hash.</param>
        /// <param name="field">The hash field to delete.</param>
        public bool RemoveHashed(string key, string field)
        {
            return _cacheProvider.RemoveHashed(key, field);
        }
        /// <summary>
        /// Returns an IList implemented using a Redis List
        /// </summary>
        /// <typeparam name="T">The object type</typeparam>
        /// <param name="key">The redis key</param>
        public ICachedList<T> GetCachedList<T>(string key)
        {
            return GetCachedCollectionProvider().GetCachedList<T>(key);
        }
        /// <summary>
        /// Returns an IDictionary implemented using a Redis Hash
        /// </summary>
        /// <typeparam name="TKey">The key type</typeparam>
        /// <typeparam name="TValue">The object type</typeparam>
        /// <param name="key">The redis key</param>
        public ICachedDictionary<TKey, TValue> GetCachedDictionary<TKey, TValue>(string key)
        {
            return GetCachedCollectionProvider().GetCachedDictionary<TKey, TValue>(key);
        }
        /// <summary>
        /// Returns an ISet implemented using a Redis Set
        /// </summary>
        /// <typeparam name="T">The object type</typeparam>
        /// <param name="key">The redis key</param>
        public ICachedSet<T> GetCachedSet<T>(string key)
        {
            return GetCachedCollectionProvider().GetCachedSet<T>(key);
        }
        #endregion
        #region Private methods
        /// <summary>
        /// Gets the cache provider as a collection provider.
        /// </summary>
        private ICachedCollectionProvider GetCachedCollectionProvider()
        {
            if (!(_cacheProvider is ICachedCollectionProvider))
            {
                throw new NotImplementedException("This cached collection is not supported by the provider.");
            }
            return (_cacheProvider as ICachedCollectionProvider);
        }
        #endregion
    }
}
