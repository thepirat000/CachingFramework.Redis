﻿using System;
using System.Collections.Generic;
using System.Linq;
using CachingFramework.Redis.Contracts.RedisObjects;
using StackExchange.Redis;
using CachingFramework.Redis.Providers;
using System.Threading.Tasks;

namespace CachingFramework.Redis.RedisObjects
{
    /// <summary>
    /// Managed list using a Redis List
    /// </summary>
    internal class RedisList<T> : RedisBaseObject, IRedisList<T>, IList<T>
    {
        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="RedisBaseObject" /> class.
        /// </summary>
        /// <param name="redisContext">The redis context.</param>
        /// <param name="redisKey">The redis key.</param>
        internal RedisList(RedisProviderContext redisContext, string redisKey)
            : base(redisContext, redisKey)
        {
        }
        #endregion

        #region IRedisList implementation
        /// <summary>
        /// Adds a range of values to the end of the list.
        /// </summary>
        /// <param name="collection">The collection.</param>
        public void AddRange(IEnumerable<T> collection)
        {
            GetRedisDb().ListRightPush(RedisKey, collection.Select(x => (RedisValue)Serialize(x)).ToArray());
        }
        /// <summary>
        /// Adds a new item to the list at the start of the list.
        /// </summary>
        /// <param name="item">The item to add</param>
        public void PushFirst(T item)
        {
            GetRedisDb().ListLeftPush(RedisKey, Serialize(item));
        }
        /// <summary>
        /// Adds a new item to the list at the end of the list (has the same effect as Add method).
        /// </summary>
        /// <param name="item">The item to add</param>
        public void PushLast(T item)
        {
            GetRedisDb().ListRightPush(RedisKey, Serialize(item));
        }
        /// <summary>
        /// Removes the item at the start of the list and returns the item removed.
        /// </summary>
        public T PopFirst()
        {
            var value = GetRedisDb().ListLeftPop(RedisKey);
            return Deserialize<T>(value);
        }
        /// <summary>
        /// Removes the item at the end of the list and returns the item removed.
        /// </summary>
        public T PopLast()
        {
            var value = GetRedisDb().ListRightPop(RedisKey);
            return Deserialize<T>(value);
        }
        /// <summary>
        /// Returns the specified elements of the list stored at key. The offsets start and stop are zero-based indexes, with 0 being the first element of the list (the head of the list), 1 being the next element and so on.
        /// These offsets can also be negative numbers indicating offsets starting at the end of the list. For example, -1 is the last element of the list, -2 the penultimate, and so on.
        /// 
        /// </summary>
        /// <param name="start">The start.</param>
        /// <param name="stop">The stop.</param>
        public IEnumerable<T> GetRange(long start = 0, long stop = -1)
        {
            return GetRedisDb().ListRange(RedisKey, start, stop).Select(Deserialize<T>);
        }
        /// <summary>
        /// Returns the first element of the list, returns the type default if the list contains no elements.
        /// </summary>
        public T FirstOrDefault()
        {
            var value = GetRedisDb().ListGetByIndex(RedisKey, 0);
            return Deserialize<T>(value);
        }
        /// <summary>
        /// Returns the last element of the list, returns the type default if the list contains no elements.
        /// </summary>
        public T LastOrDefault()
        {
            var value = GetRedisDb().ListGetByIndex(RedisKey, -1);
            return Deserialize<T>(value);
        }
        /// <summary>
        /// Inserts an item to the list at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index at which <paramref name="item" /> should be inserted.</param>
        /// <param name="item">The object to insert into the <see cref="T:System.Collections.Generic.IList`1" />.</param>
        public void Insert(long index, T item) 
        {
            if (index == 0)
            {
                PushFirst(item);
                return;
            }
            if (index == Count)
            {
                PushLast(item);
                return;
            }
            var tempKey = GetTempKey();
            var db = GetRedisDb();
            var before = db.ListGetByIndex(RedisKey, index);
            if (!before.IsNull)
            {
                var batch = db.CreateBatch();
                batch.ListSetByIndexAsync(RedisKey, index, tempKey);
                batch.ListInsertBeforeAsync(RedisKey, tempKey, Serialize(item));
                batch.ListSetByIndexAsync(RedisKey, index + 1, (byte[])before);
                batch.Execute();
            }
        }
        /// <summary>
        /// Removes the item at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the item to remove.</param>
        public void RemoveAt(long index)
        {
            if (index == 0)
            {
                PopFirst();
                return;
            }
            if (index == Count - 1)
            {
                PopLast();
                return;
            }
            var tempKey = GetTempKey();
            var batch = GetRedisDb().CreateBatch();
            batch.ListSetByIndexAsync(RedisKey, index, tempKey);
            batch.ListRemoveAsync(RedisKey, tempKey, 1);
            batch.Execute();
        }
        /// <summary>
        /// Trim an existing list so that it will contain only the specified range of elements specified. 
        /// Both start and stop are zero-based indexes, where 0 is the first element of the list (the head), 1 the next element and so on.
        /// Start and end can also be negative numbers indicating offsets from the end of the list, where -1 is the last element of the list, -2 the penultimate element and so on.
        /// </summary>
        /// <param name="start">The start zero-based index (can be negative number indicating offset from the end of the sorted set).</param>
        /// <param name="stop">The stop zero-based index (can be negative number indicating offset from the end of the sorted set).</param>
        public void Trim(long start, long stop = -1)
        {
            GetRedisDb().ListTrim(RedisKey, start, stop);
        }
        /// <summary>
        /// Removes the specified occurrences of a specific object from the <see cref="T:System.Collections.Generic.ICollection`1"/>.
        /// </summary>
        /// <param name="item">The object to remove from the <see cref="T:System.Collections.Generic.ICollection`1"/>.</param>
        /// <param name="count">if count > 0: Remove a quantity of elements equal to value moving from head to tail. if count &lt; 0: Remove elements equal to value moving from tail to head. count = 0: Remove all elements equal to value.</param>
        /// <returns>true if at least one element was successfully removed from the list.</returns>
        public bool Remove(T item, long count)
        {
            return GetRedisDb().ListRemove(RedisKey, Serialize(item), count) > 0;
        }
        #endregion

        #region IRedisListAsync implementation
        /// <summary>
        /// Adds a range of values to the end of the list.
        /// </summary>
        /// <param name="collection">The collection.</param>
        public async Task AddRangeAsync(IEnumerable<T> collection)
        {
            await GetRedisDb().ListRightPushAsync(RedisKey, collection.Select(x => (RedisValue)Serialize(x)).ToArray()).ForAwait();
        }
        /// <summary>
        /// Adds a new item to the list at the start of the list.
        /// </summary>
        /// <param name="item">The item to add</param>
        public async Task PushFirstAsync(T item)
        {
            await GetRedisDb().ListLeftPushAsync(RedisKey, Serialize(item)).ForAwait();
        }
        /// <summary>
        /// Adds a new item to the list at the end of the list (has the same effect as Add method).
        /// </summary>
        /// <param name="item">The item to add</param>
        public async Task PushLastAsync(T item)
        {
            await GetRedisDb().ListRightPushAsync(RedisKey, Serialize(item)).ForAwait();
        }
        /// <summary>
        /// Removes the item at the start of the list and returns the item removed.
        /// </summary>
        public async Task<T> PopFirstAsync()
        {
            var value = await GetRedisDb().ListLeftPopAsync(RedisKey).ForAwait();
            return Deserialize<T>(value);
        }
        /// <summary>
        /// Removes the item at the end of the list and returns the item removed.
        /// </summary>
        public async Task<T> PopLastAsync()
        {
            var value = await GetRedisDb().ListRightPopAsync(RedisKey).ForAwait();
            return Deserialize<T>(value);
        }
        /// <summary>
        /// Returns the specified elements of the list stored at key. The offsets start and stop are zero-based indexes, with 0 being the first element of the list (the head of the list), 1 being the next element and so on.
        /// These offsets can also be negative numbers indicating offsets starting at the end of the list. For example, -1 is the last element of the list, -2 the penultimate, and so on.
        /// 
        /// </summary>
        /// <param name="start">The start.</param>
        /// <param name="stop">The stop.</param>
        public async Task<IEnumerable<T>> GetRangeAsync(long start = 0, long stop = -1)
        {
            var range = await GetRedisDb().ListRangeAsync(RedisKey, start, stop).ForAwait();
            return range.Select(Deserialize<T>);
        }
        /// <summary>
        /// Inserts an item to the list at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index at which <paramref name="item" /> should be inserted.</param>
        /// <param name="item">The object to insert into the <see cref="T:System.Collections.Generic.IList`1" />.</param>
        public async Task InsertAsync(long index, T item)
        {
            if (index == 0)
            {
                await PushFirstAsync(item).ForAwait();
                return;
            }
            if (index == Count)
            {
                await PushLastAsync(item).ForAwait();
                return;
            }
            var tempKey = GetTempKey();
            var db = GetRedisDb();
            var before = await db.ListGetByIndexAsync(RedisKey, index).ForAwait();
            if (!before.IsNull)
            {
                var batch = db.CreateBatch();
#pragma warning disable 4014
                batch.ListSetByIndexAsync(RedisKey, index, tempKey);
                batch.ListInsertBeforeAsync(RedisKey, tempKey, Serialize(item));
                batch.ListSetByIndexAsync(RedisKey, index + 1, (byte[])before);
#pragma warning restore 4014
                batch.Execute();
            }
        }
        /// <summary>
        /// Removes the item at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the item to remove.</param>
        public async Task RemoveAtAsync(long index)
        {
            if (index == 0)
            {
                await PopFirstAsync().ForAwait();
                return;
            }
            if (index == Count - 1)
            {
                await PopLastAsync().ForAwait();
                return;
            }
            var tempKey = GetTempKey();
            var batch = GetRedisDb().CreateBatch();
#pragma warning disable 4014
            batch.ListSetByIndexAsync(RedisKey, index, tempKey);
            batch.ListRemoveAsync(RedisKey, tempKey, 1);
#pragma warning restore 4014
            batch.Execute();
        }
        /// <summary>
        /// Trim an existing list so that it will contain only the specified range of elements specified. 
        /// Both start and stop are zero-based indexes, where 0 is the first element of the list (the head), 1 the next element and so on.
        /// Start and end can also be negative numbers indicating offsets from the end of the list, where -1 is the last element of the list, -2 the penultimate element and so on.
        /// </summary>
        /// <param name="start">The start zero-based index (can be negative number indicating offset from the end of the sorted set).</param>
        /// <param name="stop">The stop zero-based index (can be negative number indicating offset from the end of the sorted set).</param>
        public async Task TrimAsync(long start, long stop = -1)
        {
            await GetRedisDb().ListTrimAsync(RedisKey, start, stop).ForAwait();
        }
        /// <summary>
        /// Removes the specified occurrences of a specific object from the <see cref="T:System.Collections.Generic.ICollection`1"/>.
        /// </summary>
        /// <param name="item">The object to remove from the <see cref="T:System.Collections.Generic.ICollection`1"/>.</param>
        /// <param name="count">if count > 0: Remove a quantity of elements equal to value moving from head to tail. if count &lt; 0: Remove elements equal to value moving from tail to head. count = 0: Remove all elements equal to value.</param>
        /// <returns>true if at least one element was successfully removed from the list.</returns>
        public async Task<bool> RemoveAsync(T item, long count)
        {
            var result = await GetRedisDb().ListRemoveAsync(RedisKey, Serialize(item), count).ForAwait();
            return result > 0;
        }
        /// <summary>
        /// Adds an item to the collection (has the same effect as AddLast method).
        /// </summary>
        /// <param name="item">The object to add to the <see cref="T:System.Collections.Generic.ICollection`1" />.</param>
        public async Task AddAsync(T item)
        {
            await PushLastAsync(item).ForAwait();
        }
        /// <summary>
        /// Determines whether the <see cref="T:System.Collections.Generic.ICollection`1" /> contains a specific value.
        /// </summary>
        /// <param name="item">The object to locate in the <see cref="T:System.Collections.Generic.ICollection`1" />.</param>
        /// <returns>true if <paramref name="item" /> is found in the <see cref="T:System.Collections.Generic.ICollection`1" />; otherwise, false.</returns>
        public async Task<bool> ContainsAsync(T item)
        {
            var count = Count;
            for (int i = 0; i < count; i++)
            {
                var value = await GetRedisDb().ListGetByIndexAsync(RedisKey, i).ForAwait();
                if (value.Equals(Serialize(item)))
                {
                    return true;
                }
            }
            return false;
        }
        /// <summary>
        /// Determines the index of a specific item in the <see cref="T:System.Collections.Generic.IList`1" />.
        /// </summary>
        /// <param name="item">The object to locate in the <see cref="T:System.Collections.Generic.IList`1" />.</param>
        /// <returns>The index of <paramref name="item" /> if found in the list; otherwise, -1.</returns>
        public async Task<int> IndexOfAsync(T item)
        {
            var count = Count;
            for (int i = 0; i < count; i++)
            {
                var value = await GetRedisDb().ListGetByIndexAsync(RedisKey, i).ForAwait();
                if (value.Equals(Serialize(item)))
                {
                    return i;
                }
            }
            return -1;
        }
        #endregion  

        #region IList implementation
        /// <summary>
        /// Inserts an item to the <see cref="T:System.Collections.Generic.IList`1" /> at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index at which <paramref name="item" /> should be inserted.</param>
        /// <param name="item">The object to insert into the <see cref="T:System.Collections.Generic.IList`1" />.</param>
        public void Insert(int index, T item)
        {
            Insert((long) index, item);
        }
        /// <summary>
        /// Removes the <see cref="T:System.Collections.Generic.IList`1" /> item at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the item to remove.</param>
        public void RemoveAt(int index)
        {
            RemoveAt((long) index);
        }
        /// <summary>
        /// Gets or sets the element at the specified index.
        /// </summary>
        /// <param name="index">The index.</param>
        public T this[long index]
        {
            get
            {
                return Deserialize<T>(GetRedisDb().ListGetByIndex(RedisKey, index));
            }
            set
            {
                GetRedisDb().ListSetByIndex(RedisKey, index, Serialize(value));
            }
        }
        /// <summary>
        /// Gets or sets the element at the specified index.
        /// </summary>
        /// <param name="index">The index.</param>        
        public T this[int index]
        {
            get { return this[(long) index]; }
            set { this[(long) index] = value; }
        }
        /// <summary>
        /// Adds an item to the collection (has the same effect as AddLast method).
        /// </summary>
        /// <param name="item">The object to add to the <see cref="T:System.Collections.Generic.ICollection`1" />.</param>
        public void Add(T item)
        {
            PushLast(item);
        }
        /// <summary>
        /// Determines whether the <see cref="T:System.Collections.Generic.ICollection`1" /> contains a specific value.
        /// </summary>
        /// <param name="item">The object to locate in the <see cref="T:System.Collections.Generic.ICollection`1" />.</param>
        /// <returns>true if <paramref name="item" /> is found in the <see cref="T:System.Collections.Generic.ICollection`1" />; otherwise, false.</returns>
        public bool Contains(T item)
        {
            var count = Count;
            for (int i = 0; i < count; i++)
            {
                if (GetRedisDb().ListGetByIndex(RedisKey, i).Equals(Serialize(item)))
                {
                    return true;
                }
            }
            return false;
        }
        /// <summary>
        /// Copies the entire list to a compatible one-dimensional array, starting at the specified index of the target array.
        /// </summary>
        /// <param name="array">The one-dimensional Array that is the destination of the elements copied.</param>
        /// <param name="arrayIndex">The zero-based index in array at which copying begins.</param>
        public void CopyTo(T[] array, int arrayIndex)
        {
            GetRedisDb().ListRange(RedisKey).Select(Deserialize<T>).ToArray().CopyTo(array, arrayIndex);
        }
        /// <summary>
        /// Determines the index of a specific item in the <see cref="T:System.Collections.Generic.IList`1" />.
        /// </summary>
        /// <param name="item">The object to locate in the <see cref="T:System.Collections.Generic.IList`1" />.</param>
        /// <returns>The index of <paramref name="item" /> if found in the list; otherwise, -1.</returns>
        public int IndexOf(T item)
        {
            var count = Count;
            for (int i = 0; i < count; i++)
            {
                if (GetRedisDb().ListGetByIndex(RedisKey, i).Equals(Serialize(item)))
                {
                    return i;
                }
            }
            return -1;
        }
        /// <summary>
        /// Gets the number of elements contained in the <see cref="T:System.Collections.Generic.ICollection`1" />.
        /// </summary>
        /// <value>The count.</value>
        /// <returns>The number of elements contained in the <see cref="T:System.Collections.Generic.ICollection`1" />.</returns>
        public long Count
        {
            get { return GetRedisDb().ListLength(RedisKey); }
        }
        /// <summary>
        /// Gets the number of elements contained in the collection.
        /// </summary>
        int ICollection<T>.Count
        {
            get { return (int)Count; }
        }
        /// <summary>
        /// Removes the first occurrences of a specific object from the list, moving from head to tail.
        /// </summary>
        /// <param name="item">The object to remove from the <see cref="T:System.Collections.Generic.ICollection`1"/>.</param>
        /// <returns>true if the element was successfully removed from the list.</returns>
        public bool Remove(T item)
        {
            return Remove(item, 1);
        }
        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>A <see cref="T:System.Collections.Generic.IEnumerator`1" /> that can be used to iterate through the collection.</returns>
        public IEnumerator<T> GetEnumerator()
        {
            var count = Count;
            for (int i = 0; i < count; i++)
            {
                yield return Deserialize<T>(GetRedisDb().ListGetByIndex(RedisKey, i));
            }
        }
        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>An <see cref="T:System.Collections.IEnumerator" /> object that can be used to iterate through the collection.</returns>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        #endregion

        #region Private methods
        /// <summary>
        /// Gets a temporary key.
        /// </summary>
        private string GetTempKey()
        {
            return $"TEMP_{Guid.NewGuid()}";
        }
        #endregion
    }
}
