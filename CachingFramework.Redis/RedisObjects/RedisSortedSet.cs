using System.Collections.Generic;
using System.Linq;
using CachingFramework.Redis.Contracts;
using CachingFramework.Redis.Contracts.RedisObjects;
using StackExchange.Redis;

namespace CachingFramework.Redis.RedisObjects
{
    /// <summary>
    /// Managed collection using a Redis Sorted Set
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class RedisSortedSet<T> : RedisBaseObject, ICachedSortedSet<T>, ICollection<T>
    {
        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="RedisBaseObject" /> class.
        /// </summary>
        /// <param name="connection">The connection.</param>
        /// <param name="redisKey">The redis key.</param>
        /// <param name="serializer">The serializer.</param>
        internal RedisSortedSet(ConnectionMultiplexer connection, string redisKey, ISerializer serializer)
            : base(connection, redisKey, serializer)
        {
        }
        #endregion

        #region ICachedSortedSet implementation
        /// <summary>
        /// Adds the specified member with the specified score to the sorted set stored at key. 
        /// If key does not exist, a new sorted set with the specified member as sole member is created, like if the sorted set was empty. 
        /// </summary>
        /// <param name="member">The sorted member to add.</param>
        public void Add(SortedMember<T> member)
        {
            var db = GetRedisDb();
            db.SortedSetAdd(RedisKey, Serialize(member.Value), member.Score);
        }
        /// <summary>
        /// Adds the specified member with the specified score to the sorted set stored at key. 
        /// If key does not exist, a new sorted set with the specified member as sole member is created, like if the sorted set was empty. 
        /// </summary>
        /// <param name="item">The item to add.</param>
        /// <param name="score">The item score.</param>
        public void Add(double score, T item)
        {
            Add(new SortedMember<T>(score, item));
        }
        /// <summary>
        /// Adds all the specified members with the specified scores to the sorted set stored at key. 
        /// If key does not exist, a new sorted set with the specified members as sole members is created, like if the sorted set was empty. 
        /// </summary>
        /// <param name="members">The members to add.</param>
        public void AddRange(IEnumerable<SortedMember<T>> members)
        {
            GetRedisDb().SortedSetAdd(RedisKey,
                members.Select(x => new SortedSetEntry(Serialize(x.Value), x.Score)).ToArray());
        }
        /// <summary>
        /// Returns the number of elements in the sorted set.
        /// </summary>
        public long Count
        {
            get { return GetRedisDb().SortedSetLength(RedisKey); }
        }
        /// <summary>
        /// Returns the number of elements in the sorted set at key with a score between min and max.
        /// </summary>
        /// <param name="min">The minimum score to consider (inclusive).</param>
        /// <param name="max">The maximum score to consider (inclusive).</param>
        public long CountByScore(double min, double max)
        {
            return GetRedisDb().SortedSetLength(RedisKey, min, max);
        }
        /// <summary>
        /// Returns all the elements in the sorted set at key with a score between min and max (inclusive). 
        /// </summary>
        /// <param name="min">The minimum score to consider.</param>
        /// <param name="max">The maximum score to consider.</param>
        /// <param name="descending">if set to <c>true</c> the elements are considered to be ordered from high to low scores.</param>
        /// <param name="skip">The skip number for result pagination.</param>
        /// <param name="take">The take number for result pagination.</param>
        public IEnumerable<SortedMember<T>> GetRangeByScore(double min = double.NegativeInfinity, double max = double.PositiveInfinity, bool descending = false, long skip = 0, long take = -1)
        {
            return GetRedisDb().SortedSetRangeByScoreWithScores(RedisKey, min, max, Exclude.None, descending ? Order.Descending : Order.Ascending, skip, take)
                .Select(x => new SortedMember<T>(x.Score, Deserialize<T>(x.Element)));
        }
        /// <summary>
        /// Returns the specified range of elements in the sorted set stored at key. The elements are considered to be ordered from the lowest to the highest score by default.
        /// </summary>
        /// <param name="start">The start zero-based index (can be negative number indicating offset from the end of the sorted set).</param>
        /// <param name="stop">The stop zero-based index (can be negative number indicating offset from the end of the sorted set).</param>
        /// <param name="descending">if set to <c>true</c> the elements are considered to be ordered from high to low scores.</param>
        public IEnumerable<SortedMember<T>> GetRangeByRank(long start = 0, long stop = -1, bool descending = false)
        {
            return GetRedisDb().SortedSetRangeByRankWithScores(RedisKey, start, stop, descending ? Order.Descending : Order.Ascending)
                .Select(x => new SortedMember<T>(x.Score, Deserialize<T>(x.Element)));
        }
        /// <summary>
        /// Removes all elements in the sorted set with a score between min and max (inclusive).
        /// </summary>
        /// <param name="min">The minimum score to consider.</param>
        /// <param name="max">The maximum score to consider.</param>
        public void RemoveRangeByScore(double min, double max)
        {
            GetRedisDb().SortedSetRemoveRangeByScore(RedisKey, min, max);
        }
        /// <summary>
        /// Removes all elements in the sorted set stored with rank between start and stop. 
        /// Both start and stop are zero-based indexes with 0 being the element with the lowest score. 
        /// These indexes can be negative numbers, where they indicate offsets starting at the element with the highest score. 
        /// </summary>
        /// <param name="start">The start zero-based index (can be negative number indicating offset from the end of the sorted set).</param>
        /// <param name="stop">The stop zero-based index (can be negative number indicating offset from the end of the sorted set).</param>
        public void RemoveRangeByRank(long start, long stop)
        {
            GetRedisDb().SortedSetRemoveRangeByRank(RedisKey, start, stop);
        }
        /// <summary>
        /// Increments the score of member in the sorted by the given value. 
        /// If member does not exist in the sorted set, it is added with increment as its score (as if its previous score was 0.0). 
        /// If key does not exist, a new sorted set with the specified member as its sole member is created.
        /// </summary>
        /// <param name="item">The item to increment its score.</param>
        /// <param name="value">The increment value.</param>
        /// <returns>The new score of the member</returns>
        public double IncrementScore(T item, double value)
        {
            return GetRedisDb().SortedSetIncrement(RedisKey, Serialize(item), value);
        }
        /// <summary>
        /// Returns the rank of member in the sorted set, with the scores ordered from low to high by default. 
        /// The rank (or index) is zero-based.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="descending">if set to <c>true</c> the elements are considered to be ordered from high to low scores.</param>
        /// <returns>The rank of the item in the sorted set, or NULL when the key or the member does not exists</returns>
        public long? RankOf(T item, bool descending = false)
        {
            return GetRedisDb().SortedSetRank(RedisKey, Serialize(item), descending ? Order.Descending : Order.Ascending);
        }
        /// <summary>
        /// Returns the score of member in the sorted set at key.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns>The score of the item in the sorted set, or NULL when the key or the member does not exists</returns>
        public double? ScoreOf(T item)
        {
            return GetRedisDb().SortedSetScore(RedisKey, Serialize(item));
        }
        #endregion

        #region ICollection implementation
        /// <summary>
        /// Returns an enumerator that iterates through the sorted set collection.
        /// The returned items will NOT be sorted by score.
        /// Use GetRangeByScore if you need an ordered iterator. 
        /// </summary>
        /// <returns>A <see cref="T:System.Collections.Generic.IEnumerator`1" /> that can be used to iterate through the collection.</returns>
        public IEnumerator<T> GetEnumerator()
        {
            var db = GetRedisDb();
            foreach (var item in db.SortedSetScan(RedisKey))
            {
                yield return Deserialize<T>(item.Element);
            }
        }
        /// <summary>
        /// Returns an enumerator that iterates through the sorted set collection.
        /// The returned items will NOT be sorted by score.
        /// Use GetRangeByScore if you need an ordered iterator. 
        /// </summary>
        /// <returns>An <see cref="T:System.Collections.IEnumerator" /> object that can be used to iterate through the collection.</returns>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        /// <summary>
        /// Adds the specified item with score 0, to the sorted set. 
        /// If key does not exist, a new sorted set with the specified member as sole member is created, like if the sorted set was empty. 
        /// </summary>
        /// <param name="item">The item to add.</param>
        public void Add(T item)
        {
            Add(0, item);
        }
        /// <summary>
        /// Determines whether the sorted set contains a specific item.
        /// </summary>
        /// <param name="item">The object to locate.</param>
        /// <returns>true if <paramref name="item" /> is found; otherwise, false.</returns>
        public bool Contains(T item)
        {
            return RankOf(item).HasValue;
        }
        /// <summary>
        /// Copies the entire sorted set to a compatible one-dimensional array, starting at the specified index of the target array.
        /// </summary>
        /// <param name="array">The one-dimensional Array that is the destination of the elements copied.</param>
        /// <param name="arrayIndex">The zero-based index in array at which copying begins.</param>
        public void CopyTo(T[] array, int arrayIndex)
        {
            GetRedisDb().SortedSetRangeByRank(RedisKey).Select(Deserialize<T>).ToArray().CopyTo(array, arrayIndex);
        }
        /// <summary>
        /// Gets the count.
        /// </summary>
        /// <value>The count.</value>
        int ICollection<T>.Count
        {
            get { return (int)Count; }
        }
        /// <summary>
        /// Removes the first occurrence of a specific object from the collection.
        /// </summary>
        /// <param name="item">The object to remove from the <see cref="T:System.Collections.Generic.ICollection`1" />.</param>
        /// <returns>true if <paramref name="item" /> was successfully removed from the <see cref="T:System.Collections.Generic.ICollection`1" />; otherwise, false. This method also returns false if <paramref name="item" /> is not found in the original <see cref="T:System.Collections.Generic.ICollection`1" />.</returns>
        public bool Remove(T item)
        {
            return GetRedisDb().SortedSetRemove(RedisKey, Serialize(item));
        }
        #endregion
    }
}
