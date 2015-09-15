using System.Collections.Generic;

namespace CachingFramework.Redis.Contracts
{
    /// <summary>
    /// Managed list using a Redis List
    /// </summary>
    public interface ICachedList<T> : IList<T>, IRedisObject
    {
        /// <summary>
        /// Adds a range of values to the list.
        /// </summary>
        /// <param name="collection">The collection.</param>
        void AddRange(IEnumerable<T> collection);
    }
}
