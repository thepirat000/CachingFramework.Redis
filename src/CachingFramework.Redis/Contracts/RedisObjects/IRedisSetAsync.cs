using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CachingFramework.Redis.Contracts.RedisObjects
{
    public interface IRedisSetAsync<T>
    {
        /// <summary>
        /// Adds multiple elements to the set.
        /// </summary>
        /// <param name="collection">The collection.</param>
        Task AddRangeAsync(IEnumerable<T> collection);
        /// <summary>
        /// Returns the number of elements in the set.
        /// </summary>
        Task<long> GetCountAsync();
        /// <summary>
        /// Returns and remove a random value from the set.
        /// </summary>
        Task<T> PopAsync();
        /// <summary>
        /// Returns a random value from the set.
        /// </summary>
        Task<T> GetRandomMemberAsync();
        /// <summary>
        /// Adds an item related to one or more tags.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="tags">The tags.</param>
        Task AddAsync(T item, string[] tags);
        /// <summary>
        /// Adds the specified item.
        /// </summary>
        /// <param name="item">The item.</param>
        Task<bool> AddAsync(T item);
        /// <summary>
        /// Determines whether the <see cref="T:System.Collections.Generic.ICollection`1" /> contains a specific value.
        /// </summary>
        /// <param name="item">The object to locate in the <see cref="T:System.Collections.Generic.ICollection`1" />.</param>
        /// <returns>true if <paramref name="item" /> is found in the <see cref="T:System.Collections.Generic.ICollection`1" />; otherwise, false.</returns>
        Task<bool> ContainsAsync(T item);
        /// <summary>
        /// Removes the first occurrence of a specific object from the <see cref="T:System.Collections.Generic.ICollection`1" />.
        /// </summary>
        /// <param name="item">The object to remove from the <see cref="T:System.Collections.Generic.ICollection`1" />.</param>
        /// <returns>true if <paramref name="item" /> was successfully removed from the <see cref="T:System.Collections.Generic.ICollection`1" />; otherwise, false. This method also returns false if <paramref name="item" /> is not found in the original <see cref="T:System.Collections.Generic.ICollection`1" />.</returns>
        Task<bool> RemoveAsync(T item);
        /// <summary>
        /// Removes all items from the collection
        /// </summary>
        Task ClearAsync();
    }
}
