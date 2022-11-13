using System;
using System.IO;
using CachingFramework.Redis.Contracts;
using StackExchange.Redis;

namespace CachingFramework.Redis.Providers
{
    /// <summary>
    /// Internal providers commoncontext
    /// </summary>
    internal class RedisProviderContext
    {
        /// <summary>
        /// The redis connection
        /// </summary>
        /// <value>The redis connection.</value>
        public IConnectionMultiplexer RedisConnection { get; set; }

        /// <summary>
        /// The serializer
        /// </summary>
        /// <value>The serializer.</value>
        public ISerializer Serializer { get; set; }

        /// <summary>
        /// The custom database options.
        /// </summary>
        public DatabaseOptions DatabaseOptions { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="RedisProviderContext"/> class.
        /// </summary>
        /// <param name="connection">The connection multiplexer to use.</param>
        /// <param name="serializer">The serializer.</param>
        /// <param name="databaseOptions">The custom database options.</param>
        public RedisProviderContext(IConnectionMultiplexer connection, ISerializer serializer, DatabaseOptions databaseOptions = null)
        {
            DatabaseOptions = databaseOptions ?? new DatabaseOptions();
            RedisConnection = connection;
            Serializer = serializer;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RedisProviderContext"/> class.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        /// <param name="serializer">The serializer.</param>
        /// <param name="log">The textwriter to use for logging purposes.</param>
        /// <param name="databaseOptions">The custom database options.</param>
        public RedisProviderContext(string configuration, ISerializer serializer, TextWriter log = null, DatabaseOptions databaseOptions = null)
        {
            DatabaseOptions = databaseOptions ?? new DatabaseOptions();
            RedisConnection = ConnectionMultiplexer.Connect(configuration, log);
            Serializer = serializer;
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="RedisProviderContext"/> class.
        /// </summary>
        /// <param name="configuration">The configuration object.</param>
        /// <param name="serializer">The serializer.</param>
        /// <param name="log">The textwriter to use for logging purposes.</param>
        /// <param name="databaseOptions">The custom database options.</param>
        public RedisProviderContext(ConfigurationOptions configuration, ISerializer serializer, TextWriter log = null, DatabaseOptions databaseOptions = null)
        {
            DatabaseOptions = databaseOptions ?? new DatabaseOptions();
            RedisConnection = ConnectionMultiplexer.Connect(configuration, log);
            Serializer = serializer;
        }

        /// <summary>
        /// Returns the redis database interface
        /// </summary>
        public IDatabase GetRedisDatabase()
        {
            return DatabaseOptions.GetDatabase(RedisConnection);
        }

        public override string ToString()
        {
            return $"{nameof(RedisProviderContext)} {Serializer.GetType().Name}";
        }
    }
}