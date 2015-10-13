using System.Collections.Generic;

namespace CachingFramework.Redis.Contracts.RedisObjects
{
    /// <summary>
    /// Managed collection of strings using Redis sorted set
    /// </summary>
    public interface IRedisLexicographicSet : ICollection<string>
    {
        /// <summary>
        /// Adds a range of string values to the set.
        /// </summary>
        /// <param name="collection">The collection of string to add.</param>
        void AddRange(IEnumerable<string> collection);
        /// <summary>
        /// Returns a list with the strings that starts with the specified <param name="partial"></param> string.
        /// </summary>
        /// <param name="partial">The partial string to match.</param>
        /// <param name="take">The take number for result pagination.</param>
        IEnumerable<string> AutoComplete(string partial, long take = -1);
        /// <summary>
        /// Gets the number of elements contained in the collection/>.
        /// </summary>
        new long Count { get; }
    }
}
