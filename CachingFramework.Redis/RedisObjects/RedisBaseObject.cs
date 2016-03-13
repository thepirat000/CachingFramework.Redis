using System;
using CachingFramework.Redis.Contracts;
using CachingFramework.Redis.Contracts.RedisObjects;
using StackExchange.Redis;

namespace CachingFramework.Redis.RedisObjects
{
    /// <summary>
    /// Base class for Managed collections using Redis types
    /// </summary>
    internal abstract class RedisBaseObject : IRedisObject
    {
        /// <summary>
        /// The serializer to use
        /// </summary>
        protected readonly ISerializer Serializer;
        /// <summary>
        /// The connection multiplexer
        /// </summary>
        protected readonly ConnectionMultiplexer Connection;
        /// <summary>
        /// Gets the redis key for this object
        /// </summary>
        public string RedisKey { get; protected set; }
        /// <summary>
        /// Initializes a new instance of the <see cref="RedisBaseObject"/> class.
        /// </summary>
        /// <param name="connection">The connection.</param>
        /// <param name="redisKey">The redis key.</param>
        /// <param name="serializer">The serializer.</param>
        protected RedisBaseObject(ConnectionMultiplexer connection, string redisKey, ISerializer serializer)
        {
            RedisKey = redisKey;
            Serializer = serializer;
            Connection = connection;
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="RedisBaseObject"/> class.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        /// <param name="redisKey">The redis key.</param>
        /// <param name="serializer">The serializer.</param>
        protected RedisBaseObject(string configuration, string redisKey, ISerializer serializer)
        {
            RedisKey = redisKey;
            Serializer = serializer;
            Connection = ConnectionMultiplexer.Connect(configuration);
        }
        /// <summary>
        /// Gets the redis database.
        /// </summary>
        /// <returns>IDatabase.</returns>
        internal IDatabase GetRedisDb()
        {
            return Connection.GetDatabase();
        }
        /// <summary>
        /// Serializes the specified object.
        /// </summary>
        /// <param name="obj">The object.</param>
        protected byte[] Serialize<T>(T obj)
        {
            return Serializer.Serialize(obj);
        }
        /// <summary>
        /// Deserializes the specified redis value. 
        /// Returns the type default if the value RedisValue IsNull.
        /// </summary>
        /// <param name="serialized">The serialized string.</param>
        protected T Deserialize<T>(RedisValue serialized)
        {
            return serialized.IsNull ? default(T) : Serializer.Deserialize<T>(serialized);
        }
        /// <summary>
        /// Gets or sets the time to live.
        /// Null means persistent.
        /// </summary>
        /// <value>The time to live.</value>
        public TimeSpan? TimeToLive
        {
            get { return GetRedisDb().KeyTimeToLive(RedisKey); }
            set { GetRedisDb().KeyExpire(RedisKey, value); }
        }
        /// <summary>
        /// Gets or sets the Expiration as a local datetime.
        /// Null means the key is persistent.
        /// </summary>
        public DateTime? Expiration
        {
            get { return DateTime.Now + GetRedisDb().KeyTimeToLive(RedisKey); }
            set { GetRedisDb().KeyExpire(RedisKey, value); }
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
        /// Removes all items from the collection
        /// </summary>
        public void Clear()
        {
            GetRedisDb().KeyDelete(RedisKey);
        }
    }
}
