using System;
using System.Collections.Generic;

namespace CachingFramework.Redis.Contracts.RedisObjects
{
    /// <summary>
    /// Managed HashSet using a Redis Set
    /// </summary>
    public interface IRedisSet<T> : ISet<T>, IRedisObject
    {
        /// <summary>
        /// Adds multiple elements to the set.
        /// </summary>
        /// <param name="collection">The collection.</param>
        void AddRange(IEnumerable<T> collection);
        /// <summary>
        /// Removes all the elements that meets some criteria.
        /// </summary>
        /// <param name="match">The match predicate.</param>
        int RemoveWhere(Predicate<T> match);
        /// <summary>
        /// Returns the number of elements in the set.
        /// </summary>
        new long Count { get; }
    }
}
