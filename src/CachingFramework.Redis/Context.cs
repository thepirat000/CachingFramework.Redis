using System.IO;
using CachingFramework.Redis.Contracts;
using CachingFramework.Redis.Contracts.Providers;
using CachingFramework.Redis.Providers;
using CachingFramework.Redis.Serializers;
using StackExchange.Redis;
using System;

namespace CachingFramework.Redis
{
    /// <summary>
    /// Context class containing the public APIs.
    /// NOTE: You should use 'RedisContext' class instead, since 'Context' is going to be renamed to 'RedisContext' in future versions, 
    /// so please start using `RedisContext` class to avoid breaking your code on future versions.
    /// </summary>
    [Obsolete("Context class is going to be renamed to RedisContext on future versions, please use RedisContext class instead")]
    public class Context : RedisContext
    {
        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="Context" /> class using Redis in localhost server default port 6379, and using the default Serializer.
        /// </summary>
        public Context() : base("localhost:6379") { }
#if (NET45 || NET461)
        /// <summary>
        /// Initializes a new instance of the <see cref="Context" /> class given the cache engine type and its configuration string, and using the default Serializer.
        /// </summary>
        /// <param name="configuration">The configuration string.</param>
        public Context(string configuration) : base(configuration, new BinarySerializer(), null) { }
        /// <summary>
        /// Initializes a new instance of the <see cref="Context" /> class given the cache engine type and its configuration string, and using the default Serializer.
        /// </summary>
        /// <param name="configuration">The configuration object.</param>
        public Context(ConfigurationOptions configuration) : base(configuration, new BinarySerializer(), null) { }
#else
        /// <summary>
        /// Initializes a new instance of the <see cref="Context" /> class given the cache engine type and its configuration string, and using the default Serializer.
        /// </summary>
        /// <param name="configuration">The configuration string.</param>
        public Context(string configuration) : base(configuration, new JsonSerializer(), null) { }
        /// <summary>
        /// Initializes a new instance of the <see cref="Context" /> class given the cache engine type and its configuration string, and using the default Serializer.
        /// </summary>
        /// <param name="configuration">The configuration object.</param>
        public Context(ConfigurationOptions configuration) : base(configuration, new JsonSerializer(), null) { }
#endif
        /// <summary>
        /// Initializes a new instance of the <see cref="Context" /> class.
        /// </summary>
        /// <param name="configuration">The configuration string.</param>
        /// <param name="serializer">The serializer.</param>
        public Context(string configuration, ISerializer serializer) : base(configuration, serializer, null) { }
        /// <summary>
        /// Initializes a new instance of the <see cref="Context" /> class.
        /// </summary>
        /// <param name="configuration">The configuration object.</param>
        /// <param name="serializer">The serializer.</param>
        public Context(ConfigurationOptions configuration, ISerializer serializer) : base(configuration, serializer, null) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="Context" /> class injecting the connection multiplexer and serializer to use.
        /// </summary>
        /// <param name="connection">The connection multiplexer to use.</param>
        /// <param name="serializer">The serializer.</param>
        public Context(IConnectionMultiplexer connection, ISerializer serializer) : base(connection, serializer) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="Context" /> class injecting the connection multiplexer to use.
        /// </summary>
        /// <param name="connection">The connection multiplexer to use.</param>
        /// <param name="serializer">The serializer.</param>
        public Context(IConnectionMultiplexer connection) : base(connection) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="Context" /> class.
        /// </summary>
        /// <param name="configuration">The configuration string.</param>
        /// <param name="serializer">The serializer.</param>
        /// <param name="log">The textwriter to use for logging purposes.</param>
        public Context(string configuration, ISerializer serializer, TextWriter log) : base(configuration, serializer, log) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="Context" /> class.
        /// </summary>
        /// <param name="configuration">The configuration object.</param>
        /// <param name="serializer">The serializer.</param>
        /// <param name="log">The textwriter to use for logging purposes.</param>
        public Context(ConfigurationOptions configuration, ISerializer serializer, TextWriter log) : base(configuration, serializer, log) { }

        #endregion
    }
}
