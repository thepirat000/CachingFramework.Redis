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
        /// <summary>
        /// Returns the specified elements of the list stored at key. The offsets start and stop are zero-based indexes, with 0 being the first element of the list (the head of the list), 1 being the next element and so on.
        /// These offsets can also be negative numbers indicating offsets starting at the end of the list. For example, -1 is the last element of the list, -2 the penultimate, and so on.
        /// </summary>
        /// <param name="start">The start index.</param>
        /// <param name="stop">The stop index (inclusve).</param>
        IList<T> GetRange(long start = 0, long stop = -1);
    }
}
