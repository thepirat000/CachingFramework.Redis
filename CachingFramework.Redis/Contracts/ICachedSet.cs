using System;
using System.Collections.Generic;

namespace CachingFramework.Redis.Contracts
{
    /// <summary>
    /// Managed HashSet using a Redis Set
    /// </summary>
    public interface ICachedSet<T> : ISet<T>, IRedisObject
    {
        /// <summary>
        /// Adds multiple elements to the set.
        /// </summary>
        /// <param name="collection">The collection.</param>
        void AddMultiple(IEnumerable<T> collection);
        /// <summary>
        /// Removes all the elements that meets some criteria.
        /// </summary>
        /// <param name="match">The match predicate.</param>
        int RemoveWhere(Predicate<T> match);
    }
}
