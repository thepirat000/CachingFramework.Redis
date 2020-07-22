using System;
using System.Collections.Generic;
using System.Linq;
using CachingFramework.Redis.Contracts;
using CachingFramework.Redis.Contracts.RedisObjects;
using StackExchange.Redis;
using CachingFramework.Redis.Contracts.Providers;
using CachingFramework.Redis.Providers;
using System.Threading.Tasks;

namespace CachingFramework.Redis.RedisObjects
{
    /// <summary>
    /// Managed collection using a Redis Sorted Set
    /// </summary>
    internal class RedisSet<T> : RedisBaseObject, IRedisSet<T>, ICollection<T>
    {
        #region Fields
        private readonly ICacheProvider _cacheProvider;
        private readonly int _scanPageSize;
        #endregion
        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="RedisBaseObject" /> class.
        /// </summary>
        /// <param name="redisContext">The redis context.</param>
        /// <param name="redisKey">The redis key.</param>
        /// <param name="cacheProvider">The cache provider.</param>
        /// <param name="scanPageSize">The page size for Scan operations.</param>
        internal RedisSet(RedisProviderContext redisContext, string redisKey, ICacheProvider cacheProvider, int scanPageSize)
            : base(redisContext, redisKey)
        {
            _cacheProvider = cacheProvider;
            _scanPageSize = scanPageSize;
        }
        #endregion
        #region IRedisSet implementation

        public void Add(T item, string[] tags)
        {
            if (tags == null || tags.Length == 0)
            {
                Add(item);
                return;
            }
            _cacheProvider.AddToSet<T>(RedisKey, item, tags);
        }

        /// <summary>
        /// Adds the specified items.
        /// </summary>
        /// <param name="collection">The items to add</param>
        public void AddRange(IEnumerable<T> collection)
        {
            GetRedisDb().SetAdd(RedisKey, collection.Select(x => (RedisValue)Serialize(x)).ToArray());
        }
        /// <summary>
        /// Returns and remove a random value from the set.
        /// </summary>
        public T Pop()
        {
            return Deserialize<T>(GetRedisDb().SetPop(RedisKey));
        }
        /// <summary>
        /// Returns a random value from the set.
        /// </summary>
        public T GetRandomMember()
        {
            return Deserialize<T>(GetRedisDb().SetRandomMember(RedisKey));
        }
        #endregion

        #region IRedisSetAsync implementation
        /// <summary>
        /// Adds multiple elements to the set.
        /// </summary>
        /// <param name="collection">The collection.</param>
        public async Task AddRangeAsync(IEnumerable<T> collection)
        {
            await GetRedisDb().SetAddAsync(RedisKey, collection.Select(x => (RedisValue)Serialize(x)).ToArray());
        }
        /// <summary>
        /// Returns the number of elements in the set.
        /// </summary>
        public async Task<long> GetCountAsync()
        {
            return await GetRedisDb().SetLengthAsync(RedisKey);
        }
        /// <summary>
        /// Returns and remove a random value from the set.
        /// </summary>
        public async Task<T> PopAsync()
        {
            var item = await GetRedisDb().SetPopAsync(RedisKey);
            return Deserialize<T>(item);
        }
        /// <summary>
        /// Returns a random value from the set.
        /// </summary>
        public async Task<T> GetRandomMemberAsync()
        {
            var item = await GetRedisDb().SetRandomMemberAsync(RedisKey);
            return Deserialize<T>(item);
        }
        /// <summary>
        /// Adds an item related to one or more tags.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="tags">The tags.</param>
        public async Task AddAsync(T item, string[] tags)
        {
            if (tags == null || tags.Length == 0)
            {
                await AddAsync(item);
                return;
            }
            await _cacheProvider.AddToSetAsync<T>(RedisKey, item, tags);
        }

        /// <summary>
        /// Adds the specified item.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        public async Task<bool> AddAsync(T item)
        {
            return await GetRedisDb().SetAddAsync(RedisKey, Serialize(item));
        }

        /// <summary>
        /// Determines whether the <see cref="T:System.Collections.Generic.ICollection`1" /> contains a specific value.
        /// </summary>
        /// <param name="item">The object to locate in the <see cref="T:System.Collections.Generic.ICollection`1" />.</param>
        /// <returns>true if <paramref name="item" /> is found in the <see cref="T:System.Collections.Generic.ICollection`1" />; otherwise, false.</returns>
        public async Task<bool> ContainsAsync(T item)
        {
            return await GetRedisDb().SetContainsAsync(RedisKey, Serialize(item));
        }

        /// <summary>
        /// Removes the first occurrence of a specific object from the <see cref="T:System.Collections.Generic.ICollection`1" />.
        /// </summary>
        /// <param name="item">The object to remove from the <see cref="T:System.Collections.Generic.ICollection`1" />.</param>
        /// <returns>true if <paramref name="item" /> was successfully removed from the <see cref="T:System.Collections.Generic.ICollection`1" />; otherwise, false. This method also returns false if <paramref name="item" /> is not found in the original <see cref="T:System.Collections.Generic.ICollection`1" />.</returns>
        public async Task<bool> RemoveAsync(T item)
        {
            return await GetRedisDb().SetRemoveAsync(RedisKey, Serialize(item));
        }
        #endregion

        #region ICollection implementation
        /// <summary>
        /// Adds the specified item.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        public bool Add(T item)
        {
            return GetRedisDb().SetAdd(RedisKey, Serialize(item));
        }
       
        /// <summary>
        /// Adds an item to the <see cref="T:System.Collections.Generic.ICollection`1" />.
        /// </summary>
        /// <param name="item">The object to add to the <see cref="T:System.Collections.Generic.ICollection`1" />.</param>
        void ICollection<T>.Add(T item)
        {
            Add(item);
        }
        /// <summary>
        /// Determines whether the <see cref="T:System.Collections.Generic.ICollection`1" /> contains a specific value.
        /// </summary>
        /// <param name="item">The object to locate in the <see cref="T:System.Collections.Generic.ICollection`1" />.</param>
        /// <returns>true if <paramref name="item" /> is found in the <see cref="T:System.Collections.Generic.ICollection`1" />; otherwise, false.</returns>
        public bool Contains(T item)
        {
            return GetRedisDb().SetContains(RedisKey, Serialize(item));
        }
        /// <summary>
        /// Copies the entire set to a compatible one-dimensional array, starting at the specified index of the target array.
        /// </summary>
        /// <param name="array">The one-dimensional Array that is the destination of the elements copied.</param>
        /// <param name="arrayIndex">The zero-based index in array at which copying begins.</param>
        public void CopyTo(T[] array, int arrayIndex)
        {
            GetRedisDb().SetMembers(RedisKey).Select(Deserialize<T>).ToArray().CopyTo(array, arrayIndex);
        }
        /// <summary>
        /// Gets the number of elements contained in the set.
        /// </summary>
        /// <value>The count.</value>
        /// <returns>The number of elements contained in the <see cref="T:System.Collections.Generic.ICollection`1" />.</returns>
        public long Count
        {
            get { return GetRedisDb().SetLength(RedisKey); }
        }
        /// <summary>
        /// Gets the number of elements contained in the set.
        /// </summary>
        int ICollection<T>.Count
        {
            get { return (int)Count; }
        }
        /// <summary>
        /// Removes the first occurrence of a specific object from the <see cref="T:System.Collections.Generic.ICollection`1" />.
        /// </summary>
        /// <param name="item">The object to remove from the <see cref="T:System.Collections.Generic.ICollection`1" />.</param>
        /// <returns>true if <paramref name="item" /> was successfully removed from the <see cref="T:System.Collections.Generic.ICollection`1" />; otherwise, false. This method also returns false if <paramref name="item" /> is not found in the original <see cref="T:System.Collections.Generic.ICollection`1" />.</returns>
        public bool Remove(T item)
        {
            return GetRedisDb().SetRemove(RedisKey, Serialize(item));
        }
        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>A <see cref="T:System.Collections.Generic.IEnumerator`1" /> that can be used to iterate through the collection.</returns>
        public IEnumerator<T> GetEnumerator()
        {
            foreach (var item in GetRedisDb().SetScan(RedisKey, default(RedisValue), _scanPageSize))
            {
                yield return Deserialize<T>(item);
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
