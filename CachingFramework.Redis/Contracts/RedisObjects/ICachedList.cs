using System.Collections.Generic;

namespace CachingFramework.Redis.Contracts.RedisObjects
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
        /// Adds a new item to the list at the start of the list.
        /// </summary>
        /// <param name="item">The item to add</param>
        void AddFirst(T item);
        /// <summary>
        /// Adds a new item to the list at the end of the list (has the same effect as Add method).
        /// </summary>
        /// <param name="item">The item to add</param>
        void AddLast(T item);
        /// <summary>
        /// Removes the item at the start of the list and returns the item removed.
        /// </summary>
        T RemoveFirst();
        /// <summary>
        /// Removes the item at the end of the list and returns the item removed.
        /// </summary>
        T RemoveLast();
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
        T First { get; }
        /// <summary>
        /// Returns the last element of the list, returns the type default if the list contains no elements.
        /// </summary>
        T Last { get; }
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
    }
}
