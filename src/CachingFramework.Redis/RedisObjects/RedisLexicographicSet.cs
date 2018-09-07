using System.Collections.Generic;
using System.Linq;
using CachingFramework.Redis.Contracts;
using CachingFramework.Redis.Contracts.RedisObjects;
using StackExchange.Redis;
using CachingFramework.Redis.Providers;

namespace CachingFramework.Redis.RedisObjects
{
    /// <summary>
    /// Managed string collection using a Redis Sorted Set with lexicographical order
    /// </summary>
    internal class RedisLexicographicSet : RedisBaseObject, IRedisLexicographicSet, ICollection<string>
    {
        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="RedisLexicographicSet" /> class.
        /// </summary>
        /// <param name="redisContext">The redis context.</param>
        /// <param name="redisKey">The redis key.</param>
        internal RedisLexicographicSet(RedisProviderContext redisContext, string redisKey)
            : base(redisContext, redisKey)
        {
        }
        #endregion

        #region IRedisLexicographicSet implementation
        /// <summary>
        /// Adds a range of string values to the set.
        /// </summary>
        /// <param name="collection">The collection of string to add.</param>
        public void AddRange(IEnumerable<string> collection)
        {
            GetRedisDb().SortedSetAdd(RedisKey, collection.Select(x => new SortedSetEntry(x, 0)).ToArray());
        }
        /// <summary>
        /// Returns the strings that starts with the specified partial string.
        /// </summary>
        /// <param name="partial">The partial string to match.</param>
        /// <param name="take">The take number for result pagination.</param>
        /// <returns>IEnumerable{System.String}.</returns>
        public IEnumerable<string> AutoComplete(string partial, long take = -1)
        {
            return GetRedisDb()
                    .SortedSetRangeByValue(RedisKey, partial, partial + (char) 255, Exclude.None, Order.Ascending, 0, take)
                    .Select(value => (string)value);
        }
        /// <summary>
        /// Iterates over the strings that matches the specified glob-style pattern.
        /// </summary>
        /// <param name="pattern">The glob-style pattern.</param>
        public IEnumerable<string> Match(string pattern)
        {
            return GetRedisDb().SortedSetScan(RedisKey, pattern).Select(x => (string) x.Element);
        }
        #endregion

        #region ICollection implementation
        /// <summary>
        /// Adds an item to the <see cref="T:System.Collections.Generic.ICollection`1" />.
        /// </summary>
        public void Add(string item)
        {
            GetRedisDb().SortedSetAdd(RedisKey, item, 0);
        }
        /// <summary>
        /// Determines whether the set contains a specific value.
        /// </summary>
        /// <param name="item">The string to locate.</param>
        public bool Contains(string item)
        {
            return GetRedisDb().SortedSetRangeByValue(RedisKey, item, item, Exclude.None, Order.Ascending, 0, 1).Length > 0;
        }
        /// <summary>
        /// Copies the entire sorted set to a compatible one-dimensional array, starting at the specified index of the target array.
        /// </summary>
        /// <param name="array">The one-dimensional Array that is the destination of the elements copied.</param>
        /// <param name="arrayIndex">The zero-based index in array at which copying begins.</param>
        public void CopyTo(string[] array, int arrayIndex)
        {
            GetRedisDb().SortedSetRangeByRank(RedisKey).Select(x => (string) x).ToArray().CopyTo(array, arrayIndex);
        }
        /// <summary>
        /// Gets the number of elements contained in the collection/>.
        /// </summary>
        public long Count
        {
            get { return GetRedisDb().SortedSetLength(RedisKey); }
        }
        /// <summary>
        /// Gets the number of elements contained in the collection.
        /// </summary>
        int ICollection<string>.Count
        {
            get { return (int)Count; }
        }
        /// <summary>
        /// Removes the first occurrence of a specific object from the collection.
        /// </summary>
        /// <param name="item">The string to remove.</param>
        /// <returns>true if <paramref name="item" /> was successfully removed; otherwise, false. This method also returns false if <paramref name="item" /> is not found.</returns>
        public bool Remove(string item)
        {
            return GetRedisDb().SortedSetRemove(RedisKey, item);
        }
        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>A <see cref="T:System.Collections.Generic.IEnumerator`1" /> that can be used to iterate through the collection.</returns>
        public IEnumerator<string> GetEnumerator()
        {
            var db = GetRedisDb();
            foreach (var item in db.SortedSetScan(RedisKey))
            {
                yield return item.Element;
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
