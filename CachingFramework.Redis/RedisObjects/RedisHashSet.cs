using System;
using System.Collections.Generic;
using System.Linq;
using CachingFramework.Redis.Contracts;
using CachingFramework.Redis.Serializers;
using StackExchange.Redis;

namespace CachingFramework.Redis.RedisObjects
{
    /// <summary>
    /// Managed hashset using a Redis Hash
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class RedisHashSet<T> : RedisBaseObject, ICachedSet<T>
    {
        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="RedisBaseObject" /> class.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        /// <param name="redisKey">The redis key.</param>
        public RedisHashSet(string configuration, string redisKey)
            : base(configuration, redisKey)
        {
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="RedisBaseObject" /> class.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        /// <param name="redisKey">The redis key.</param>
        /// <param name="serializer">The serializer.</param>
        public RedisHashSet(string configuration, string redisKey, ISerializer serializer)
            : base(configuration, redisKey, serializer)
        {
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="RedisBaseObject" /> class.
        /// </summary>
        /// <param name="connection">The connection.</param>
        /// <param name="redisKey">The redis key.</param>
        internal RedisHashSet(ConnectionMultiplexer connection, string redisKey)
            : base(connection, redisKey, new BinarySerializer())
        {
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="RedisBaseObject" /> class.
        /// </summary>
        /// <param name="connection">The connection.</param>
        /// <param name="redisKey">The redis key.</param>
        /// <param name="serializer">The serializer.</param>
        internal RedisHashSet(ConnectionMultiplexer connection, string redisKey, ISerializer serializer)
            : base(connection, redisKey, serializer)
        {
        }
        #endregion
        #region ICachedSet implementation
        /// <summary>
        /// Adds the specified item.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        public bool Add(T item)
        {
            var db = GetRedisDb();
            return db.SetAdd(RedisKey, Serialize(item));
        }
        /// <summary>
        /// Adds the specified items.
        /// </summary>
        /// <param name="collection">The items to add</param>
        public void AddMultiple(IEnumerable<T> collection)
        {
            var db = GetRedisDb();
            db.SetAdd(RedisKey, collection.Select(x => (RedisValue)Serialize(x)).ToArray());
        }
        /// <summary>
        /// Removes all elements in the specified collection from the current set.
        /// </summary>
        /// <param name="other">The other set.</param>
        public void ExceptWith(IEnumerable<T> other)
        {
            var db = GetRedisDb();
            db.SetRemove(RedisKey, other.Select(x => (RedisValue) Serialize(x)).ToArray());
        }
        /// <summary>
        /// Modifies the current set so that it contains only elements that are also in a specified collection.
        /// </summary>
        /// <param name="other">The other set.</param>
        public void IntersectWith(IEnumerable<T> other)
        {
            this.RemoveWhere(x => !other.Contains(x));
        }
        /// <summary>
        /// Determines whether [is proper subset of] [the specified other].
        /// </summary>
        /// <param name="other">The collection to copmare to the current set.</param>
        /// <returns><c>true</c> if [is proper subset of] [the specified other]; otherwise, <c>false</c>.</returns>
        public bool IsProperSubsetOf(IEnumerable<T> other)
        {
            return IsSubsetOf(true, other);
        }
        /// <summary>
        /// Determines whether [is proper superset of] [the specified other].
        /// </summary>
        /// <param name="other">The collection to copmare to the current set.</param>
        /// <returns><c>true</c> if [is proper superset of] [the specified other]; otherwise, <c>false</c>.</returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public bool IsProperSupersetOf(IEnumerable<T> other)
        {
            return IsSupersetOf(true, other);
        }
        /// <summary>
        /// Determines whether [is subset of] [the specified other].
        /// </summary>
        /// <param name="other">The collection to copmare to the current set.</param>
        /// <returns><c>true</c> if [is subset of] [the specified other]; otherwise, <c>false</c>.</returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public bool IsSubsetOf(IEnumerable<T> other)
        {
            return IsSubsetOf(false, other);
        }
        /// <summary>
        /// Determines whether [is superset of] [the specified other].
        /// </summary>
        /// <param name="other">The collection to copmare to the current set.</param>
        /// <returns><c>true</c> if [is superset of] [the specified other]; otherwise, <c>false</c>.</returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public bool IsSupersetOf(IEnumerable<T> other)
        {
            return IsSupersetOf(false, other);
        }
        /// <summary>
        /// Determines whether the current set overlaps with the specified collection.
        /// </summary>
        /// <param name="other">The collection to copmare to the current set.</param>
        /// <returns>true if the current set and other share at least one common element; otherwise, false.</returns>
        public bool Overlaps(IEnumerable<T> other)
        {
            foreach (var item in other)
            {
                if (this.Contains(item))
                {
                    return true;
                }
            }
            return false;
        }
        /// <summary>
        /// Determines whether the current set and the specified collection contain the same elements.
        /// </summary>
        /// <param name="other">The collection to compare to the current set.</param>
        public bool SetEquals(IEnumerable<T> other)
        {
            if (other is ISet<T>)
            {
                return SetEquals((ISet<T>) other);
            }
            int count = 0;
            foreach (var item in other)
            {
                if (!this.Contains(item))
                {
                    return false;
                }
                count++;
            }
            return count == this.Count;
        }
        /// <summary>
        /// Determines whether the current set and the specified collection contain the same elements.
        /// </summary>
        /// <param name="other">The collection to compare to the current set.</param>
        private bool SetEquals(ISet<T> other)
        {
            int otherLen = other.Count;
            int thisLen = this.Count;
            if (otherLen != thisLen)
            {
                return false;
            }
            return other.All(this.Contains);
        }
        /// <summary>
        /// Modifies the current set so that it contains only elements that are present either in the current set or in the specified collection, but not both.
        /// </summary>
        /// <param name="other">The collection to compare to the current set.</param>
        public void SymmetricExceptWith(IEnumerable<T> other)
        {
            foreach (var item in other)
            {
                if (this.Contains(item))
                {
                    this.Remove(item);
                }
                else
                {
                    this.Add(item);
                }
            }
        }
        /// <summary>
        /// Modifies the current set so that it contains all elements that are present in the current set, in the specified collection, or in both.
        /// </summary>
        /// <param name="other">The other.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        public void UnionWith(IEnumerable<T> other)
        {
            foreach (var item in other)
            {
                if (!this.Contains(item))
                {
                    this.Add(item);
                }
            }
        }
        /// <summary>
        /// Adds an item to the <see cref="T:System.Collections.Generic.ICollection`1" />.
        /// </summary>
        /// <param name="item">The object to add to the <see cref="T:System.Collections.Generic.ICollection`1" />.</param>
        void ICollection<T>.Add(T item)
        {
            Add(item);
        }
        /// <summary>
        /// Removes all items from the <see cref="T:System.Collections.Generic.ICollection`1" />.
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
            return GetRedisDb().SetContains(RedisKey, Serialize(item));
        }
        /// <summary>
        /// Copies the entire set to a compatible one-dimensional array, starting at the specified index of the target array.
        /// </summary>
        /// <param name="array">The one-dimensional Array that is the destination of the elements copied.</param>
        /// <param name="arrayIndex">The zero-based index in array at which copying begins.</param>
        public void CopyTo(T[] array, int arrayIndex)
        {
            GetRedisDb().SetMembers(RedisKey).Select(x => Deserialize<T>(x)).ToArray().CopyTo(array, arrayIndex);
        }
        /// <summary>
        /// Gets the number of elements contained in the <see cref="T:System.Collections.Generic.ICollection`1" />.
        /// </summary>
        /// <value>The count.</value>
        /// <returns>The number of elements contained in the <see cref="T:System.Collections.Generic.ICollection`1" />.</returns>
        public int Count
        {
            get { return (int)GetRedisDb().SetLength(RedisKey); }
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
            return GetRedisDb().SetRemove(RedisKey, Serialize(item));
        }
        /// <summary>
        /// Removes all the elements that meets some criteria.
        /// </summary>
        /// <param name="match">The match predicate.</param>
        public int RemoveWhere(Predicate<T> match)
        {
            return (int)GetRedisDb().SetRemove(RedisKey, this.Where(x => match(x)).Select(x => (RedisValue)Serialize(x)).ToArray());
        }
        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>A <see cref="T:System.Collections.Generic.IEnumerator`1" /> that can be used to iterate through the collection.</returns>
        public IEnumerator<T> GetEnumerator()
        {
            var db = GetRedisDb();
            foreach (var item in db.SetScan(RedisKey))
            {
                yield return Deserialize<T>(item);
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
        /// Determines whether [is subset of] [the specified proper].
        /// </summary>
        /// <param name="proper">if set to <c>true</c> [proper].</param>
        /// <param name="other">The other.</param>
        /// <returns><c>true</c> if [is subset of] [the specified proper]; otherwise, <c>false</c>.</returns>
        private bool IsSubsetOf(bool proper, IEnumerable<T> other)
        {
            if (other is ISet<T>)
            {
                return IsSubsetOf(proper, (ISet<T>)other);
            }
            return IsSubsetOf(proper, new HashSet<T>(other));
        }
        /// <summary>
        /// Determines whether [is subset of] [the specified proper].
        /// </summary>
        /// <param name="proper">if set to <c>true</c> [proper].</param>
        /// <param name="other">The other.</param>
        /// <returns><c>true</c> if [is subset of] [the specified proper]; otherwise, <c>false</c>.</returns>
        private bool IsSubsetOf(bool proper, ISet<T> other)
        {
            int currentLen = this.Count;
            int otherLen = other.Count;
            if (otherLen < currentLen)
            {
                return false;
            }
            if (proper && otherLen == currentLen)
            {
                return false;
            }
            foreach (var item in this)
            {
                if (!other.Contains(item))
                {
                    return false;
                }
            }
            return true;
        }
        /// <summary>
        /// Determines whether [is superset of] [the specified proper].
        /// </summary>
        /// <param name="proper">if set to <c>true</c> [proper].</param>
        /// <param name="other">The other.</param>
        /// <returns><c>true</c> if [is superset of] [the specified proper]; otherwise, <c>false</c>.</returns>
        private bool IsSupersetOf(bool proper, IEnumerable<T> other)
        {
            if (other is ISet<T>)
            {
                return IsSupersetOf(proper, (ISet<T>) other);
            }
            return IsSupersetOf(proper, new HashSet<T>(other));
        }
        /// <summary>
        /// Determines whether [is superset of] [the specified proper].
        /// </summary>
        /// <param name="proper">if set to <c>true</c> [proper].</param>
        /// <param name="other">The other.</param>
        /// <returns><c>true</c> if [is superset of] [the specified proper]; otherwise, <c>false</c>.</returns>
        private bool IsSupersetOf(bool proper, ISet<T> other)
        {
            int currentLen = this.Count;
            int otherLen = other.Count;
            if (currentLen < otherLen)
            {
                return false;
            }
            if (proper && otherLen == currentLen)
            {
                return false;
            }
            foreach (var item in other)
            {
                if (!this.Contains(item))
                {
                    return false;
                }
            }
            return true;
        }
        #endregion
    }
}
