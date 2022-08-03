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
        /// Initializes a new instance of the <see cref="RedisProviderContext"/> class.
        /// </summary>
        /// <param name="connection">The connection multiplexer to use.</param>
        /// <param name="serializer">The serializer.</param>
        public RedisProviderContext(IConnectionMultiplexer connection, ISerializer serializer)
        {
            RedisConnection = connection;
            Serializer = serializer;
        }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="RedisProviderContext"/> class.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        /// <param name="serializer">The serializer.</param>
        /// <param name="log">The textwriter to use for logging purposes.</param>
        public RedisProviderContext(string configuration, ISerializer serializer, TextWriter log = null)
        {
            RedisConnection = ConnectionMultiplexer.Connect(configuration, log);
            Serializer = serializer;
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="RedisProviderContext"/> class.
        /// </summary>
        /// <param name="configuration">The configuration object.</param>
        /// <param name="serializer">The serializer.</param>
        /// <param name="log">The textwriter to use for logging purposes.</param>
        public RedisProviderContext(ConfigurationOptions configuration, ISerializer serializer, TextWriter log = null)
        {
            RedisConnection = ConnectionMultiplexer.Connect(configuration, log);
            Serializer = serializer;
        }
    }
}