using System.Collections.Generic;
using System.Linq;
using CachingFramework.Redis.Contracts;
using CachingFramework.Redis.Serializers;
using StackExchange.Redis;

namespace CachingFramework.Redis.RedisObjects
{
    /// <summary>
    /// Managed list using a Redis List
    /// </summary>
    internal class RedisList<T> : RedisBaseObject, ICachedList<T>
    {
        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="RedisBaseObject" /> class.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        /// <param name="redisKey">The redis key.</param>
        public RedisList(string configuration, string redisKey)
            : base(configuration, redisKey)
        {
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="RedisBaseObject" /> class.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        /// <param name="redisKey">The redis key.</param>
        /// <param name="serializer">The serializer.</param>
        public RedisList(string configuration, string redisKey, ISerializer serializer)
            : base(configuration, redisKey, serializer)
        {
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="RedisBaseObject" /> class.
        /// </summary>
        /// <param name="connection">The connection.</param>
        /// <param name="redisKey">The redis key.</param>
        internal RedisList(ConnectionMultiplexer connection, string redisKey)
            : base(connection, redisKey, new BinarySerializer())
        {
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="RedisBaseObject" /> class.
        /// </summary>
        /// <param name="connection">The connection.</param>
        /// <param name="redisKey">The redis key.</param>
        /// <param name="serializer">The serializer.</param>
        internal RedisList(ConnectionMultiplexer connection, string redisKey, ISerializer serializer)
            : base(connection, redisKey, serializer)
        {
        }
        #endregion
        #region ICachedList implementation
        /// <summary>
        /// Adds a range of values to the end of the list.
        /// </summary>
        /// <param name="collection">The collection.</param>
        public void AddRange(IEnumerable<T> collection)
        {
            var db = GetRedisDb();
            db.ListRightPush(RedisKey, collection.Select(x => (RedisValue)Serialize(x)).ToArray());
        }
        /// <summary>
        /// Returns the specified elements of the list stored at key. The offsets start and stop are zero-based indexes, with 0 being the first element of the list (the head of the list), 1 being the next element and so on.
        /// These offsets can also be negative numbers indicating offsets starting at the end of the list. For example, -1 is the last element of the list, -2 the penultimate, and so on.
        /// 
        /// </summary>
        /// <param name="start">The start.</param>
        /// <param name="stop">The stop.</param>
        /// <returns>IList{`0}.</returns>
        public IList<T> GetRange(long start = 0, long stop = -1)
        {
            var db = GetRedisDb();
            return db.ListRange(RedisKey, start, stop).Select(x => Deserialize<T>(x)).ToList();
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
            var db = GetRedisDb();
            var before = db.ListGetByIndex(RedisKey, index);
            db.ListInsertBefore(RedisKey, before, Serialize(item));
        }
        /// <summary>
        /// Removes the <see cref="T:System.Collections.Generic.IList`1" /> item at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the item to remove.</param>
        public void RemoveAt(int index)
        {
            var db = GetRedisDb();
            var value = db.ListGetByIndex(RedisKey, index);
            if (!value.IsNull)
            {
                db.ListRemove(RedisKey, value);
            }
        }
        /// <summary>
        /// Gets or sets the element at the specified index.
        /// </summary>
        /// <param name="index">The index.</param>
        public T this[int index]
        {
            get
            {
                var redisValue = GetRedisDb().ListGetByIndex(RedisKey, index);
                return redisValue.IsNull ? default(T) : Deserialize<T>(redisValue);
            }
            set
            {
                Insert(index, value);
            }
        }
        /// <summary>
        /// Adds an item to the collection.
        /// </summary>
        /// <param name="item">The object to add to the <see cref="T:System.Collections.Generic.ICollection`1" />.</param>
        public void Add(T item)
        {
            GetRedisDb().ListRightPush(RedisKey, Serialize(item));
        }
        /// <summary>
        /// Removes all items from the collection.
        /// </summary>
        public void Clear()
        {
            GetRedisDb().KeyDelete(RedisKey);
        }
        /// <summary>
        /// Determines whether the <see cref="T:System.Collections.Generic.ICollection`1" /> contains a specific value.
        /// </summary>
        /// <param name="item">The object to locate in the <see cref="T:System.Collections.Generic.ICollection`1" />.</param>
        /// <returns>true if <paramref name="item" /> is found in the <see cref="T:System.Collections.Generic.ICollection`1" />; otherwise, false.</returns>
        public bool Contains(T item)
        {
            for (int i = 0; i < Count; i++)
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
            GetRedisDb().ListRange(RedisKey).Select(x => Deserialize<T>(x)).ToArray().CopyTo(array, arrayIndex);
        }
        /// <summary>
        /// Determines the index of a specific item in the <see cref="T:System.Collections.Generic.IList`1" />.
        /// </summary>
        /// <param name="item">The object to locate in the <see cref="T:System.Collections.Generic.IList`1" />.</param>
        /// <returns>The index of <paramref name="item" /> if found in the list; otherwise, -1.</returns>
        public int IndexOf(T item)
        {
            for (int i = 0; i < Count; i++)
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
        public int Count
        {
            get { return (int)GetRedisDb().ListLength(RedisKey); }
        }
        /// <summary>
        /// Gets a value indicating whether the <see cref="T:System.Collections.Generic.ICollection`1" /> is read-only.
        /// </summary>
        /// <value><c>true</c> if this instance is read only; otherwise, <c>false</c>.</value>
        /// <returns>true if the <see cref="T:System.Collections.Generic.ICollection`1" /> is read-only; otherwise, false.</returns>
        public bool IsReadOnly
        {
            get { return false; }
        }
        /// <summary>
        /// Removes the first occurrence of a specific object from the <see cref="T:System.Collections.Generic.ICollection`1" />.
        /// </summary>
        /// <param name="item">The object to remove from the <see cref="T:System.Collections.Generic.ICollection`1" />.</param>
        /// <returns>true if <paramref name="item" /> was successfully removed from the <see cref="T:System.Collections.Generic.ICollection`1" />; otherwise, false. This method also returns false if <paramref name="item" /> is not found in the original <see cref="T:System.Collections.Generic.ICollection`1" />.</returns>
        public bool Remove(T item)
        {
            return GetRedisDb().ListRemove(RedisKey, Serialize(item)) > 0;
        }
        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>A <see cref="T:System.Collections.Generic.IEnumerator`1" /> that can be used to iterate through the collection.</returns>
        public IEnumerator<T> GetEnumerator()
        {
            for (int i = 0; i < Count; i++)
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
            return this.GetEnumerator();
        }
        #endregion
    }
}
