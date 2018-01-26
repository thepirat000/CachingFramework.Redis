using System.Collections.Generic;

namespace CachingFramework.Redis.Contracts.RedisObjects
{
    /// <summary>
    /// Managed dictionary using a Redis Hash
    /// </summary>
    public interface IRedisDictionary<TK, TV> : IRedisDictionaryAsync<TK, TV>, IDictionary<TK, TV>, IRedisObject
    {
        /// <summary>
        /// Adds a single element to the dictionary related to the given tag(s).
        /// </summary>
        /// <param name="key">The redis key.</param>
        /// <param name="value">The value.</param>
        /// <param name="tags">The tags to relate.</param>
        void Add(TK key, TV value, string[] tags);
        /// <summary>
        /// Adds multiple elements to the dictionary.
        /// </summary>
        /// <param name="collection">The collection.</param>
        void AddRange(IEnumerable<KeyValuePair<TK, TV>> collection);
        /// <summary>
        /// Returns the number of elements in the hash.
        /// </summary>
        new long Count { get; }
        /// <summary>
        /// Increments the number stored at the hash field key. 
        /// If the key does not exist, it is set to 0 before performing the operation. 
        /// An exception is thrown if the key content can not be represented as integer.
        /// </summary>
        /// <param name="key">The hash field key to increment</param>
        /// <param name="increment">The increment value</param>
        /// <returns>The value represented in the string after the increment.</returns>
        /// <remarks>You should use this method only when the serialization of the values can be represented as raw integers. i.e. This method will not work with BinarySerialization</remarks>
        long IncrementBy(TK key, long increment);
        /// <summary>
        /// Increment the floating point number stored at the hash field key. 
        /// If the key does not exist, it is set to 0 before performing the operation. 
        /// An exception is thrown if the key content is not parsable as a double precision floating point number.
        /// </summary>
        /// <param name="key">The hash field key to increment</param>
        /// <param name="increment">The increment value</param>
        /// <returns>The value represented in the string after the increment.</returns>
        /// <remarks>You should use this method only when the serialization of the values can be represented as raw integers. i.e. This method will not work with BinarySerialization</remarks>
        double IncrementByFloat(TK key, double increment);
        /// <summary>
        /// Gets the value associated with the specified key.
        /// </summary>
        /// <param name="key">The key whose value to get.</param>
        /// <returns>The value stored at key</returns>
        TV GetValue(TK key);
    }
}
