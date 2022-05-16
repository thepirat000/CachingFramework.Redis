using System.Collections.Generic;
using System.Linq;
using CachingFramework.Redis.Contracts;
using CachingFramework.Redis.Contracts.RedisObjects;
using StackExchange.Redis;
using When = CachingFramework.Redis.Contracts.When;
using CachingFramework.Redis.Providers;
using CachingFramework.Redis.Contracts.Providers;
using System.Threading.Tasks;

namespace CachingFramework.Redis.RedisObjects
{
    /// <summary>
    /// Managed collection using a Redis Sorted Set
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class RedisSortedSet<T> : RedisBaseObject, IRedisSortedSet<T>, ICollection<T>
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
        internal RedisSortedSet(RedisProviderContext redisContext, string redisKey, ICacheProvider cacheProvider, int scanPageSize)
            : base(redisContext, redisKey)
        {
            _cacheProvider = cacheProvider;
            _scanPageSize = scanPageSize;
        }
        #endregion

        #region IRedisSortedSet implementation
        /// <summary>
        /// Adds the specified member with the specified score to the sorted set stored at key. 
        /// If key does not exist, a new sorted set with the specified member as sole member is created, like if the sorted set was empty. 
        /// </summary>
        /// <param name="member">The sorted member to add.</param>
        /// <param name="when">Indicates when this operation should be performed.</param>
        public void Add(SortedMember<T> member, When when = When.Always)
        {
            var db = GetRedisDb();
            if (when == When.Always)
            {
                db.SortedSetAdd(RedisKey, Serialize(member.Value), member.Score);
            }
            else
            {
                db.ScriptEvaluate(LuaScriptResource.Zadd, new RedisKey[] { RedisKey }, new RedisValue[] { when == When.Exists ? "XX" : "NX", member.Score, Serialize(member.Value) });
            }
        }
        /// <summary>
        /// Adds the specified member with the specified score to the sorted set stored at key. 
        /// If key does not exist, a new sorted set with the specified member as sole member is created, like if the sorted set was empty. 
        /// </summary>
        /// <param name="member">The sorted member to add.</param>
        /// <param name="when">Indicates when this operation should be performed.</param>
        public async Task AddAsync(SortedMember<T> member, When when = When.Always)
        {
            var db = GetRedisDb();
            if (when == When.Always)
            {
                await db.SortedSetAddAsync(RedisKey, Serialize(member.Value), member.Score).ForAwait();
            }
            else
            {
                await db.ScriptEvaluateAsync(LuaScriptResource.Zadd, new RedisKey[] { RedisKey }, new RedisValue[] { when == When.Exists ? "XX" : "NX", member.Score, Serialize(member.Value) }).ForAwait();
            }
        }
        /// <summary>
        /// Adds the specified member with the specified score to the sorted set stored at key. 
        /// If key does not exist, a new sorted set with the specified member as sole member is created, like if the sorted set was empty. 
        /// </summary>
        /// <param name="item">The item to add.</param>
        /// <param name="score">The item score.</param>
        /// <param name="when">Indicates when this operation should be performed.</param>
        public void Add(double score, T item, When when = When.Always)
        {
            Add(new SortedMember<T>(score, item), when);
        }
        /// <summary>
        /// Adds the specified member with the specified score to the sorted set stored at key. 
        /// If key does not exist, a new sorted set with the specified member as sole member is created, like if the sorted set was empty. 
        /// </summary>
        /// <param name="item">The item to add.</param>
        /// <param name="score">The item score.</param>
        /// <param name="when">Indicates when this operation should be performed.</param>
        public async Task AddAsync(double score, T item, When when = When.Always)
        {
            await AddAsync(new SortedMember<T>(score, item), when).ForAwait();
        }

        public void Add(SortedMember<T> member, string[] tags)
        {
            Add(member.Score, member.Value, tags);
        }

        public async Task AddAsync(SortedMember<T> member, string[] tags)
        {
            await AddAsync(member.Score, member.Value, tags).ForAwait();
        }

        public void Add(double score, T item, string[] tags)
        {
            if (tags == null || tags.Length == 0)
            {
                Add(score, item);
                return;
            }
            _cacheProvider.AddToSortedSet(RedisKey, score, item, tags);
        }

        public async Task AddAsync(double score, T item, string[] tags)
        {
            if (tags == null || tags.Length == 0)
            {
                await AddAsync(score, item).ForAwait();
                return;
            }
            await _cacheProvider.AddToSortedSetAsync(RedisKey, score, item, tags).ForAwait();
        }

        /// <summary>
        /// Adds all the specified members with the specified scores to the sorted set stored at key. 
        /// If key does not exist, a new sorted set with the specified members as sole members is created, like if the sorted set was empty. 
        /// </summary>
        /// <param name="members">The members to add.</param>
        /// <param name="when">Indicates when this operation should be performed.</param>
        public void AddRange(IEnumerable<SortedMember<T>> members, When when = When.Always)
        {
            var db = GetRedisDb();
            if (when == When.Always)
            {
                db.SortedSetAdd(RedisKey, members.Select(x => new SortedSetEntry(Serialize(x.Value), x.Score)).ToArray());
            }
            else
            {
                var @params = new List<RedisValue>();
                @params.Add(when == When.Exists ? "XX" : "NX");
                foreach (var x in members)
                {
                    @params.Add(x.Score);
                    @params.Add(Serialize(x.Value));
                }
                db.ScriptEvaluate(LuaScriptResource.Zadd, new RedisKey[] { RedisKey }, @params.ToArray());
            }
        }

        /// <summary>
        /// Adds all the specified members with the specified scores to the sorted set stored at key. 
        /// If key does not exist, a new sorted set with the specified members as sole members is created, like if the sorted set was empty. 
        /// </summary>
        /// <param name="members">The members to add.</param>
        /// <param name="when">Indicates when this operation should be performed.</param>
        public async Task AddRangeAsync(IEnumerable<SortedMember<T>> members, When when = When.Always)
        {
            var db = GetRedisDb();
            if (when == When.Always)
            {
                await db.SortedSetAddAsync(RedisKey, members.Select(x => new SortedSetEntry(Serialize(x.Value), x.Score)).ToArray()).ForAwait();
            }
            else
            {
                var @params = new List<RedisValue>();
                @params.Add(when == When.Exists ? "XX" : "NX");
                foreach (var x in members)
                {
                    @params.Add(x.Score);
                    @params.Add(Serialize(x.Value));
                }
                await db.ScriptEvaluateAsync(LuaScriptResource.Zadd, new RedisKey[] { RedisKey }, @params.ToArray()).ForAwait();
            }
        }

        /// <summary>
        /// Returns the number of elements in the sorted set.
        /// </summary>
        public long Count
        {
            get { return GetRedisDb().SortedSetLength(RedisKey); }
        }
        
        /// <summary>
        /// Returns the number of elements in the sorted set.
        /// </summary>
        public async Task<long> CountAsync()
        {
            return await GetRedisDb().SortedSetLengthAsync(RedisKey).ForAwait();
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
        /// Returns the number of elements in the sorted set at key with a score between min and max.
        /// </summary>
        /// <param name="min">The minimum score to consider (inclusive).</param>
        /// <param name="max">The maximum score to consider (inclusive).</param>
        public async Task<long> CountByScoreAsync(double min, double max)
        {
            return await GetRedisDb().SortedSetLengthAsync(RedisKey, min, max).ForAwait();
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
        /// Returns all the elements in the sorted set at key with a score between min and max (inclusive). 
        /// </summary>
        /// <param name="min">The minimum score to consider.</param>
        /// <param name="max">The maximum score to consider.</param>
        /// <param name="descending">if set to <c>true</c> the elements are considered to be ordered from high to low scores.</param>
        /// <param name="skip">The skip number for result pagination.</param>
        /// <param name="take">The take number for result pagination.</param>
        public async Task<IEnumerable<SortedMember<T>>> GetRangeByScoreAsync(double min = double.NegativeInfinity, double max = double.PositiveInfinity, bool descending = false, long skip = 0, long take = -1)
        {
            return (await GetRedisDb().SortedSetRangeByScoreWithScoresAsync(RedisKey, min, max, Exclude.None, descending ? Order.Descending : Order.Ascending, skip, take).ForAwait())
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
        /// Returns the specified range of elements in the sorted set stored at key. The elements are considered to be ordered from the lowest to the highest score by default.
        /// </summary>
        /// <param name="start">The start zero-based index (can be negative number indicating offset from the end of the sorted set).</param>
        /// <param name="stop">The stop zero-based index (can be negative number indicating offset from the end of the sorted set).</param>
        /// <param name="descending">if set to <c>true</c> the elements are considered to be ordered from high to low scores.</param>
        public async Task<IEnumerable<SortedMember<T>>> GetRangeByRankAsync(long start = 0, long stop = -1, bool descending = false)
        {
            return (await GetRedisDb().SortedSetRangeByRankWithScoresAsync(RedisKey, start, stop, descending ? Order.Descending : Order.Ascending).ForAwait())
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
        /// Removes all elements in the sorted set with a score between min and max (inclusive).
        /// </summary>
        /// <param name="min">The minimum score to consider.</param>
        /// <param name="max">The maximum score to consider.</param>
        public async Task RemoveRangeByScoreAsync(double min, double max)
        {
            await GetRedisDb().SortedSetRemoveRangeByScoreAsync(RedisKey, min, max).ForAwait();
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
        /// Removes all elements in the sorted set stored with rank between start and stop. 
        /// Both start and stop are zero-based indexes with 0 being the element with the lowest score. 
        /// These indexes can be negative numbers, where they indicate offsets starting at the element with the highest score. 
        /// </summary>
        /// <param name="start">The start zero-based index (can be negative number indicating offset from the end of the sorted set).</param>
        /// <param name="stop">The stop zero-based index (can be negative number indicating offset from the end of the sorted set).</param>
        public async Task RemoveRangeByRankAsync(long start, long stop)
        {
            await GetRedisDb().SortedSetRemoveRangeByRankAsync(RedisKey, start, stop).ForAwait();
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
        /// Increments the score of member in the sorted by the given value. 
        /// If member does not exist in the sorted set, it is added with increment as its score (as if its previous score was 0.0). 
        /// If key does not exist, a new sorted set with the specified member as its sole member is created.
        /// </summary>
        /// <param name="item">The item to increment its score.</param>
        /// <param name="value">The increment value.</param>
        /// <returns>The new score of the member</returns>
        public async Task<double> IncrementScoreAsync(T item, double value)
        {
            return await GetRedisDb().SortedSetIncrementAsync(RedisKey, Serialize(item), value).ForAwait();
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
        /// Returns the rank of member in the sorted set, with the scores ordered from low to high by default. 
        /// The rank (or index) is zero-based.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="descending">if set to <c>true</c> the elements are considered to be ordered from high to low scores.</param>
        /// <returns>The rank of the item in the sorted set, or NULL when the key or the member does not exists</returns>
        public async Task<long?> RankOfAsync(T item, bool descending = false)
        {
            return await GetRedisDb().SortedSetRankAsync(RedisKey, Serialize(item), descending ? Order.Descending : Order.Ascending).ForAwait();
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

        /// <summary>
        /// Returns the score of member in the sorted set at key.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns>The score of the item in the sorted set, or NULL when the key or the member does not exists</returns>
        public async Task<double?> ScoreOfAsync(T item)
        {
            return await GetRedisDb().SortedSetScoreAsync(RedisKey, Serialize(item)).ForAwait();
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
            foreach (var item in GetRedisDb().SortedSetScan(RedisKey, default(RedisValue), _scanPageSize))
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
        /// Adds the specified item with score 0, to the sorted set. 
        /// If key does not exist, a new sorted set with the specified member as sole member is created, like if the sorted set was empty. 
        /// </summary>
        /// <param name="item">The item to add.</param>
        public async Task AddAsync(T item)
        {
            await AddAsync(0, item).ForAwait();
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
        /// Determines whether the sorted set contains a specific item.
        /// </summary>
        /// <param name="item">The object to locate.</param>
        /// <returns>true if <paramref name="item" /> is found; otherwise, false.</returns>
        public async Task<bool> ContainsAsync(T item)
        {
            return (await RankOfAsync(item).ForAwait()).HasValue;
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
        /// <summary>
        /// Removes the first occurrence of a specific object from the collection.
        /// </summary>
        /// <param name="item">The object to remove from the <see cref="T:System.Collections.Generic.ICollection`1" />.</param>
        /// <returns>true if <paramref name="item" /> was successfully removed from the <see cref="T:System.Collections.Generic.ICollection`1" />; otherwise, false. This method also returns false if <paramref name="item" /> is not found in the original <see cref="T:System.Collections.Generic.ICollection`1" />.</returns>
        public async Task<bool> RemoveAsync(T item)
        {
            return await GetRedisDb().SortedSetRemoveAsync(RedisKey, Serialize(item)).ForAwait();
        }
        #endregion
    }
}
