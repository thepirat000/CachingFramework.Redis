using System;
using CachingFramework.Redis.Serializers;
using StackExchange.Redis;

namespace CachingFramework.Redis.Contracts
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
        /// The redis key
        /// </summary>
        protected readonly string RedisKey;
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
        protected IDatabase GetRedisDb()
        {
            return Connection.GetDatabase();
        }
        /// <summary>
        /// Serializes the specified object.
        /// </summary>
        /// <param name="obj">The object.</param>
        protected byte[] Serialize(object obj)
        {
            return Serializer.Serialize(obj);
        }
        /// <summary>
        /// Deserializes the specified string.
        /// </summary>
        /// <param name="serialized">The serialized string.</param>
        protected T Deserialize<T>(byte[] serialized)
        {
            return Serializer.Deserialize<T>(serialized);
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
    }
}
