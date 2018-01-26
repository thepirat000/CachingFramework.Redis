using System.Collections.Generic;
using System.Threading.Tasks;

namespace CachingFramework.Redis.Contracts.RedisObjects
{
    /// <summary>
    /// Managed string using a Redis string.
    /// Redis Strings are limited to 512 megabytes
    /// </summary>
    public interface IRedisStringAsync
    {
        /// <summary>
        /// Appends the value at the end of the string. 
        /// </summary>
        /// <param name="value">The value to append.</param>
        Task<long> AppendAsync(string value);
        /// <summary>
        /// Overwrites the entire string stored at key. 
        /// </summary>
        /// <param name="value">The new string to write.</param>
        Task SetAsync(string value);
        /// <summary>
        /// Overwrites the integer stored at key. 
        /// </summary>
        /// <param name="value">The new integer to write.</param>
        Task SetAsync(long value);
        /// <summary>
        /// Overwrites the floating point number stored at key. 
        /// </summary>
        /// <param name="value">The new number to write.</param>
        Task SetAsync(double value);
        /// <summary>
        /// Overwrites the entire string stored at key and returns the old value stored at key. 
        /// </summary>
        /// <param name="value">The new string to write.</param>
        Task<string> GetSetAsync(string value);
        /// <summary>
        /// Overwrites the entire integer stored at key and returns the old value stored at key. 
        /// </summary>
        /// <param name="value">The new integer to write.</param>
        Task<long> GetSetAsync(long value);
        /// <summary>
        /// Overwrites the entire floating point number stored at key and returns the old value stored at key. 
        /// </summary>
        /// <param name="value">The new number to write.</param>
        Task<double> GetSetAsync(double value);
        /// <summary>
        /// Overwrites part of the string stored at key, starting at the specified offset, for the entire length of value. 
        /// If the offset is larger than the current length of the string at key, the string is padded with zero-bytes to make offset fit. 
        /// Non-existing keys are considered as empty strings, so this command will make sure it holds a string large enough to be able to set value at offset.
        /// </summary>
        /// <param name="offset">The zero-based offset in bytes.</param>
        /// <param name="value">The string to write.</param>
        /// <returns>The length of the string after it was modified by the command</returns>
        Task<long> SetRangeAsync(long offset, string value);
        /// <summary>
        /// Returns the substring of the string value stored at key, determined by the offsets start and stop (both are inclusive). 
        /// Negative offsets can be used in order to provide an offset starting from the end of the string. So -1 means the last character.
        /// </summary>
        /// <param name="start">The start zero-based index (can be negative number indicating offset from the end of the sorted set).</param>
        /// <param name="stop">The stop zero-based index (can be negative number indicating offset from the end of the sorted set).</param>
        Task<string> GetRangeAsync(long start = 0, long stop = -1);
        /// <summary>
        /// Increments the number stored at key by increment. 
        /// If the key does not exist, it is set to 0 before performing the operation. 
        /// An exception is thrown if the key content can not be represented as integer.
        /// </summary>
        /// <param name="increment">The increment value</param>
        /// <returns>The value represented in the string after the increment.</returns>
        Task<long> IncrementByAsync(long increment);
        /// <summary>
        /// Increment the string representing a floating point number stored at key by the specified increment. 
        /// If the key does not exist, it is set to 0 before performing the operation. 
        /// An exception is thrown if the key content is not parsable as a double precision floating point number.
        /// </summary>
        /// <param name="increment">The increment value</param>
        /// <returns>The value represented in the string after the increment.</returns>
        Task<double> IncrementByFloatAsync(double increment);
        /// <summary>
        /// Clears the string
        /// </summary>
        Task ClearAsync();
        /// <summary>
        /// Returns the contents of the string as an integer value.
        /// Returns the default when the key does not exists.
        /// Throws an exception if the string value cannot be parsed as an integer
        /// </summary>
        /// <param name="default">The default value to return when the key does not exists (default is 0).</param>
        Task<long> AsIntegerAsync(long @default = 0);
        /// <summary>
        /// Returns the contents of the string as a floating point value.
        /// Returns the default when the key does not exists.
        /// Throws an exception if the string value cannot be parsed as a double precision floating point number.
        /// </summary>
        /// <param name="default">The default value to return when the key does not exists (default is 0).</param>
        Task<double> AsFloatAsync(double @default = 0);
        /// <summary>
        /// Returns the string that represents this instance.
        /// </summary>
        Task<string> ToStringAsync();
    }
}
