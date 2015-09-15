using System.Collections.Generic;

namespace CachingFramework.Redis.Contracts
{
    /// <summary>
    /// Managed dictionary using a Redis Hash
    /// </summary>
    public interface ICachedDictionary<TKey, TValue> : IDictionary<TKey, TValue>, IRedisObject
    {
        /// <summary>
        /// Adds multiple elements to the dictionary.
        /// </summary>
        /// <param name="collection">The collection.</param>
        void AddMultiple(IEnumerable<KeyValuePair<TKey, TValue>> collection);
    }
}
