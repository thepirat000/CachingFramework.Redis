using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CachingFramework.Redis.Contracts.RedisObjects
{
    /// <summary>
    /// Async implementation of a Redis List
    /// </summary>
    public interface IRedisListAsync<T> 
    {
        /// <summary>
        /// Adds a range of values to the end of the list.
        /// </summary>
        /// <param name="collection">The collection.</param>
        Task AddRangeAsync(IEnumerable<T> collection);
        /// <summary>
        /// Adds a new item to the list at the start of the list.
        /// </summary>
        /// <param name="item">The item to add</param>
        Task PushFirstAsync(T item);
        /// <summary>
        /// Adds a new item to the list at the end of the list (has the same effect as Add method).
        /// </summary>
        /// <param name="item">The item to add</param>
        Task PushLastAsync(T item);
        /// <summary>
        /// Removes and return the item at the start of the list.
        /// </summary>
        Task<T> PopFirstAsync();
        /// <summary>
        /// Removes and return the item at the end of the list.
        /// </summary>
        Task<T> PopLastAsync();
        /// <summary>
        /// Returns the specified elements of the list stored at key. The offsets start and stop are zero-based indexes, with 0 being the first element of the list (the head of the list), 1 being the next element and so on.
        /// These offsets can also be negative numbers indicating offsets starting at the end of the list. For example, -1 is the last element of the list, -2 the penultimate, and so on.
        /// </summary>
        /// <param name="start">The start index.</param>
        /// <param name="stop">The stop index (inclusve).</param>
        Task<IEnumerable<T>> GetRangeAsync(long start = 0, long stop = -1);
        /// <summary>
        /// Inserts an item to the <see cref="T:System.Collections.Generic.IList`1" /> at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index at which <paramref name="item" /> should be inserted.</param>
        /// <param name="item">The object to insert into the <see cref="T:System.Collections.Generic.IList`1" />.</param>
        Task InsertAsync(long index, T item);
        /// <summary>
        /// Removes the item at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the item to remove.</param>
        Task RemoveAtAsync(long index);
        /// <summary>
        /// Trim an existing list so that it will contain only the specified range of elements specified. 
        /// Both start and stop are zero-based indexes, where 0 is the first element of the list (the head), 1 the next element and so on.
        /// Start and end can also be negative numbers indicating offsets from the end of the list, where -1 is the last element of the list, -2 the penultimate element and so on.
        /// </summary>
        /// <param name="start">The start zero-based index (can be negative number indicating offset from the end of the sorted set).</param>
        /// <param name="stop">The stop zero-based index (can be negative number indicating offset from the end of the sorted set).</param>
        Task TrimAsync(long start, long stop = -1);
        /// <summary>
        /// Removes the specified occurrences of a specific object from the <see cref="T:System.Collections.Generic.ICollection`1"/>.
        /// </summary>
        /// <param name="item">The object to remove from the <see cref="T:System.Collections.Generic.ICollection`1"/>.</param>
        /// <param name="count">if count > 0: Remove a quantity of elements equal to value moving from head to tail. if count &lt; 0: Remove elements equal to value moving from tail to head. count = 0: Remove all elements equal to value.</param>
        /// <returns>true if at least one element was successfully removed from the list.</returns>
        Task<bool> RemoveAsync(T item, long count);
        /// <summary>
        /// Adds an item to the collection (has the same effect as AddLast method).
        /// </summary>
        /// <param name="item">The object to add to the <see cref="T:System.Collections.Generic.ICollection`1" />.</param>
        Task AddAsync(T item);
        /// <summary>
        /// Determines whether the <see cref="T:System.Collections.Generic.ICollection`1" /> contains a specific value.
        /// </summary>
        /// <param name="item">The object to locate in the <see cref="T:System.Collections.Generic.ICollection`1" />.</param>
        /// <returns>true if <paramref name="item" /> is found in the <see cref="T:System.Collections.Generic.ICollection`1" />; otherwise, false.</returns>
        Task<bool> ContainsAsync(T item);
        /// <summary>
        /// Determines the index of a specific item in the <see cref="T:System.Collections.Generic.IList`1" />.
        /// </summary>
        /// <param name="item">The object to locate in the <see cref="T:System.Collections.Generic.IList`1" />.</param>
        /// <returns>The index of <paramref name="item" /> if found in the list; otherwise, -1.</returns>
        Task<int> IndexOfAsync(T item);
        /// <summary>
        /// Removes all items from the collection
        /// </summary>
        Task ClearAsync();
    }
}
