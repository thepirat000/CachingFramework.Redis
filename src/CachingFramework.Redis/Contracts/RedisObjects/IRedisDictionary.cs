using System.Collections.Generic;

namespace CachingFramework.Redis.Contracts.RedisObjects
{
    /// <summary>
    /// Managed dictionary using a Redis Hash
    /// </summary>
    public interface IRedisDictionary<TK, TV> : IDictionary<TK, TV>, IRedisObject
    {
        /// <summary>
        /// Adds multiple elements to the dictionary.
        /// </summary>
        /// <param name="collection">The collection.</param>
        void AddRange(IEnumerable<KeyValuePair<TK, TV>> collection);
        /// <summary>
        /// Returns the number of elements in the hash.
        /// </summary>
        new long Count { get; }
    }
}
