using System;
using System.Collections.Generic;

namespace CachingFramework.Redis.Contracts.RedisObjects
{
    /// <summary>
    /// Managed HashSet using a Redis Set
    /// </summary>
    public interface IRedisSet<T> : IRedisSetAsync<T>, ICollection<T>, IRedisObject
    {
        /// <summary>
        /// Adds multiple elements to the set.
        /// </summary>
        /// <param name="collection">The collection.</param>
        void AddRange(IEnumerable<T> collection);
        /// <summary>
        /// Returns the number of elements in the set.
        /// </summary>
        new long Count { get; }
        /// <summary>
        /// Returns and remove a random value from the set.
        /// </summary>
        T Pop();
        /// <summary>
        /// Returns a random value from the set.
        /// </summary>
        T GetRandomMember();
        /// <summary>
        /// Adds an item related to one or more tags.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="tags">The tags.</param>
        void Add(T item, string[] tags);
    }
}
