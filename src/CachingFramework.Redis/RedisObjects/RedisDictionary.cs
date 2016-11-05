using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CachingFramework.Redis.Contracts.RedisObjects;
using StackExchange.Redis;
using CachingFramework.Redis.Contracts.Providers;
using CachingFramework.Redis.Providers;

namespace CachingFramework.Redis.RedisObjects
{
    /// <summary>
    /// Managed dictionary using a Redis Hash
    /// </summary>
    internal class RedisDictionary<TK, TV> : RedisBaseObject, IRedisDictionary<TK, TV>, IDictionary<TK, TV>
    {
        #region Fields
        private readonly ICacheProvider _cacheProvider;
        #endregion
        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="RedisBaseObject" /> class.
        /// </summary>
        /// <param name="redisContext">The redis context.</param>
        /// <param name="redisKey">The redis key.</param>
        /// <param name="cacheProvider">The cache provider.</param>
        internal RedisDictionary(RedisProviderContext redisContext, string redisKey, ICacheProvider cacheProvider)
            : base(redisContext, redisKey)
        {
            _cacheProvider = cacheProvider;
        }
        #endregion
        #region IRedisDictionary implementation
        /// <summary>
        /// Adds multiple elements to the dictionary.
        /// </summary>
        /// <param name="items">The items to add.</param>
        public void AddRange(IEnumerable<KeyValuePair<TK, TV>> items)
        {
            GetRedisDb()
                .HashSet(RedisKey, items.Select(i => new HashEntry(Serialize(i.Key), Serialize(i.Value))).ToArray());
        }

        public void Add(TK key, TV value, string[] tags)
        {
            if (tags == null || tags.Length == 0)
            {
                Add(key, value);
                return;
            }
            _cacheProvider.SetHashed<TK, TV>(RedisKey, key, value, tags);
        }
        #endregion
        #region IDictionary implementation
        /// <summary>
        /// Adds an element with the provided key and value to the <see cref="T:System.Collections.Generic.IDictionary`2" />.
        /// </summary>
        /// <param name="key">The object to use as the key of the element to add.</param>
        /// <param name="value">The object to use as the value of the element to add.</param>
        public void Add(TK key, TV value)
        {
            GetRedisDb().HashSet(RedisKey, Serialize(key), Serialize(value));
        }
        /// <summary>
        /// Determines whether the <see cref="T:System.Collections.Generic.IDictionary`2" /> contains an element with the specified key.
        /// </summary>
        /// <param name="key">The key to locate in the <see cref="T:System.Collections.Generic.IDictionary`2" />.</param>
        /// <returns>true if the <see cref="T:System.Collections.Generic.IDictionary`2" /> contains an element with the key; otherwise, false.</returns>
        public bool ContainsKey(TK key)
        {
            return GetRedisDb().HashExists(RedisKey, Serialize(key));
        }
        /// <summary>
        /// Removes the element with the specified key from the <see cref="T:System.Collections.Generic.IDictionary`2" />.
        /// </summary>
        /// <param name="key">The key of the element to remove.</param>
        /// <returns>true if the element is successfully removed; otherwise, false.  This method also returns false if <paramref name="key" /> was not found in the original <see cref="T:System.Collections.Generic.IDictionary`2" />.</returns>
        public bool Remove(TK key)
        {
            return GetRedisDb().HashDelete(RedisKey, Serialize(key));
        }
        /// <summary>
        /// Gets the value associated with the specified key.
        /// </summary>
        /// <param name="key">The key whose value to get.</param>
        /// <param name="value">When this method returns, the value associated with the specified key, if the key is found; otherwise, the default value for the type of the <paramref name="value" /> parameter. This parameter is passed uninitialized.</param>
        /// <returns>true if the object that implements <see cref="T:System.Collections.Generic.IDictionary`2" /> contains an element with the specified key; otherwise, false.</returns>
        public bool TryGetValue(TK key, out TV value)
        {
            var redisValue = GetRedisDb().HashGet(RedisKey, Serialize(key));
            if (redisValue.IsNull)
            {
                value = default(TV);
                return false;
            }
            value = Deserialize<TV>(redisValue);
            return true;
        }
        /// <summary>
        /// Gets an <see cref="T:System.Collections.Generic.ICollection`1" /> containing the values in the <see cref="T:System.Collections.Generic.IDictionary`2" />.
        /// </summary>
        /// <value>The values.</value>
        /// <returns>An <see cref="T:System.Collections.Generic.ICollection`1" /> containing the values in the object that implements <see cref="T:System.Collections.Generic.IDictionary`2" />.</returns>
        public ICollection<TV> Values
        {
            get { return new Collection<TV>(GetRedisDb().HashValues(RedisKey).Select(Deserialize<TV>).ToList()); }
        }
        /// <summary>
        /// Gets an <see cref="T:System.Collections.Generic.ICollection`1" /> containing the keys of the <see cref="T:System.Collections.Generic.IDictionary`2" />.
        /// </summary>
        /// <value>The keys.</value>
        /// <returns>An <see cref="T:System.Collections.Generic.ICollection`1" /> containing the keys of the object that implements <see cref="T:System.Collections.Generic.IDictionary`2" />.</returns>
        public ICollection<TK> Keys
        {
            get { return new Collection<TK>(GetRedisDb().HashKeys(RedisKey).Select(Deserialize<TK>).ToList()); }
        }
        /// <summary>
        /// Gets or sets the element at the specified key.
        /// </summary>
        /// <param name="key">The key.</param>
        public TV this[TK key]
        {
            get
            {
                var redisValue = GetRedisDb().HashGet(RedisKey, Serialize(key));
                return Deserialize<TV>(redisValue);
            }
            set
            {
                Add(key, value);
            }
        }
        /// <summary>
        /// Adds an item to the <see cref="T:System.Collections.Generic.ICollection`1" />.
        /// </summary>
        /// <param name="item">The object to add to the <see cref="T:System.Collections.Generic.ICollection`1" />.</param>
        public void Add(KeyValuePair<TK, TV> item)
        {
            Add(item.Key, item.Value);
        }
        /// <summary>
        /// Determines whether the <see cref="T:System.Collections.Generic.ICollection`1" /> contains a specific value.
        /// </summary>
        /// <param name="item">The object to locate in the <see cref="T:System.Collections.Generic.ICollection`1" />.</param>
        /// <returns>true if <paramref name="item" /> is found in the <see cref="T:System.Collections.Generic.ICollection`1" />; otherwise, false.</returns>
        public bool Contains(KeyValuePair<TK, TV> item)
        {
            return GetRedisDb().HashExists(RedisKey, Serialize(item.Key));
        }
        /// <summary>
        /// Copies the entire dictionary to a compatible one-dimensional array, starting at the specified index of the target array.
        /// </summary>
        /// <param name="array">The one-dimensional Array that is the destination of the elements copied.</param>
        /// <param name="arrayIndex">The zero-based index in array at which copying begins.</param>
        public void CopyTo(KeyValuePair<TK, TV>[] array, int arrayIndex)
        {
            GetRedisDb().HashGetAll(RedisKey).Select(x => new KeyValuePair<TK, TV>(Deserialize<TK>(x.Name), Deserialize<TV>(x.Value))).ToArray().CopyTo(array, arrayIndex);
        }
        /// <summary>
        /// Gets the number of elements contained in the hash.
        /// </summary>
        /// <value>The count.</value>
        /// <returns>The number of elements contained in the <see cref="T:System.Collections.Generic.ICollection`1" />.</returns>
        public long Count
        {
            get { return GetRedisDb().HashLength(RedisKey); }
        }
        /// <summary>
        /// Gets the number of elements contained in the hash.
        /// </summary>
        int ICollection<KeyValuePair<TK, TV>>.Count
        {
            get { return (int)Count; }
        }
        /// <summary>
        /// Removes the first occurrence of a specific object from the <see cref="T:System.Collections.Generic.ICollection`1" />.
        /// </summary>
        /// <param name="item">The object to remove from the <see cref="T:System.Collections.Generic.ICollection`1" />.</param>
        /// <returns>true if <paramref name="item" /> was successfully removed from the <see cref="T:System.Collections.Generic.ICollection`1" />; otherwise, false. This method also returns false if <paramref name="item" /> is not found in the original <see cref="T:System.Collections.Generic.ICollection`1" />.</returns>
        public bool Remove(KeyValuePair<TK, TV> item)
        {
            return Remove(item.Key);
        }
        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>A <see cref="T:System.Collections.Generic.IEnumerator`1" /> that can be used to iterate through the collection.</returns>
        public IEnumerator<KeyValuePair<TK, TV>> GetEnumerator()
        {
            var db = GetRedisDb();
            foreach (var hashEntry in db.HashScan(RedisKey))
            {
                yield return new KeyValuePair<TK, TV>(Deserialize<TK>(hashEntry.Name), Deserialize<TV>(hashEntry.Value));
            }
        }
        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>An <see cref="T:System.Collections.IEnumerator" /> object that can be used to iterate through the collection.</returns>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        #endregion
    }
}
