using System.Collections.Generic;
using System.Threading.Tasks;

namespace CachingFramework.Redis.Contracts.RedisObjects
{
    /// <summary>
    /// Managed list using a Redis List
    /// </summary>
    public interface IRedisList<T> : IRedisListAsync<T>, IList<T>, IRedisObject
    {
        /// <summary>
        /// Adds a range of values to the end of the list.
        /// </summary>
        /// <param name="collection">The collection.</param>
        void AddRange(IEnumerable<T> collection);
        /// <summary>
        /// Adds a new item to the list at the start of the list.
        /// </summary>
        /// <param name="item">The item to add</param>
        void PushFirst(T item);
        /// <summary>
        /// Adds a new item to the list at the end of the list (has the same effect as Add method).
        /// </summary>
        /// <param name="item">The item to add</param>
        void PushLast(T item);
        /// <summary>
        /// Removes and return the item at the start of the list.
        /// </summary>
        T PopFirst();
        /// <summary>
        /// Removes and return the item at the end of the list.
        /// </summary>
        T PopLast();
        /// <summary>
        /// Returns the specified elements of the list stored at key. The offsets start and stop are zero-based indexes, with 0 being the first element of the list (the head of the list), 1 being the next element and so on.
        /// These offsets can also be negative numbers indicating offsets starting at the end of the list. For example, -1 is the last element of the list, -2 the penultimate, and so on.
        /// </summary>
        /// <param name="start">The start index.</param>
        /// <param name="stop">The stop index (inclusve).</param>
        IEnumerable<T> GetRange(long start = 0, long stop = -1);
        /// <summary>
        /// Returns the number of elements in the list.
        /// </summary>
        new long Count { get; }
        /// <summary>
        /// Returns the first element of the list, returns the type default if the list contains no elements.
        /// </summary>
        T FirstOrDefault();
        /// <summary>
        /// Returns the last element of the list, returns the type default if the list contains no elements.
        /// </summary>
        T LastOrDefault();
        /// <summary>
        /// Inserts an item to the <see cref="T:System.Collections.Generic.IList`1" /> at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index at which <paramref name="item" /> should be inserted.</param>
        /// <param name="item">The object to insert into the <see cref="T:System.Collections.Generic.IList`1" />.</param>
        void Insert(long index, T item);
        /// <summary>
        /// Removes the item at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the item to remove.</param>
        void RemoveAt(long index);
        /// <summary>
        /// Gets or sets the element at the specified index.
        /// </summary>
        /// <param name="index">The index.</param>
        T this[long index] { get; set; }
        /// <summary>
        /// Trim an existing list so that it will contain only the specified range of elements specified. 
        /// Both start and stop are zero-based indexes, where 0 is the first element of the list (the head), 1 the next element and so on.
        /// Start and end can also be negative numbers indicating offsets from the end of the list, where -1 is the last element of the list, -2 the penultimate element and so on.
        /// </summary>
        /// <param name="start">The start zero-based index (can be negative number indicating offset from the end of the sorted set).</param>
        /// <param name="stop">The stop zero-based index (can be negative number indicating offset from the end of the sorted set).</param>
        void Trim(long start, long stop = -1);
        /// <summary>
        /// Removes the specified occurrences of a specific object from the <see cref="T:System.Collections.Generic.ICollection`1"/>.
        /// </summary>
        /// <param name="item">The object to remove from the <see cref="T:System.Collections.Generic.ICollection`1"/>.</param>
        /// <param name="count">if count > 0: Remove a quantity of elements equal to value moving from head to tail. if count &lt; 0: Remove elements equal to value moving from tail to head. count = 0: Remove all elements equal to value.</param>
        /// <returns>true if at least one element was successfully removed from the list.</returns>
        bool Remove(T item, long count);
    }
}
