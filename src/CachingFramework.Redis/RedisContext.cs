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
    /// </summary>
    public class RedisContext : IContext, IDisposable
    {
        #region Fields
        private readonly RedisProviderContext _internalContext;
        /// <summary>
        /// The cache provider
        /// </summary>
        private readonly ICacheProvider _cacheProvider;
        /// <summary>
        /// The collection provider
        /// </summary>
        private readonly ICollectionProvider _collectionProvider;
        /// <summary>
        /// The geo spatial provider
        /// </summary>
        private readonly IGeoProvider _geoProvider;
        /// <summary>
        /// The pub/sub provider
        /// </summary>
        private readonly IPubSubProvider _pubsubProvider;

        private readonly IKeyEventsProvider _keyEventsProvider;
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="Context" /> class using Redis in localhost server default port 6379, and using the default Serializer.
        /// </summary>
        public RedisContext() : this("localhost:6379") { }
#if (NET45 || NET461)
        /// <summary>
        /// Initializes a new instance of the <see cref="Context" /> class given the cache engine type and its configuration string, and using the default Serializer.
        /// </summary>
        /// <param name="configuration">The configuration string.</param>
        public RedisContext(string configuration) : this(configuration, new BinarySerializer(), null) { }
        /// <summary>
        /// Initializes a new instance of the <see cref="Context" /> class given the cache engine type and its configuration string, and using the default Serializer.
        /// </summary>
        /// <param name="configuration">The configuration object.</param>
        public RedisContext(ConfigurationOptions configuration) : this(configuration, new BinarySerializer(), null) { }
#else
        /// <summary>
        /// Initializes a new instance of the <see cref="Context" /> class given the cache engine type and its configuration string, and using the default Serializer.
        /// </summary>
        /// <param name="configuration">The configuration string.</param>
        public RedisContext(string configuration) : this(configuration, new JsonSerializer(), null) { }
        /// <summary>
        /// Initializes a new instance of the <see cref="Context" /> class given the cache engine type and its configuration string, and using the default Serializer.
        /// </summary>
        /// <param name="configuration">The configuration object.</param>
        public RedisContext(ConfigurationOptions configuration) : this(configuration, new JsonSerializer(), null) { }
#endif
        /// <summary>
        /// Initializes a new instance of the <see cref="Context" /> class.
        /// </summary>
        /// <param name="configuration">The configuration string.</param>
        /// <param name="serializer">The serializer.</param>
        public RedisContext(string configuration, ISerializer serializer) : this(configuration, serializer, null) { }
        /// <summary>
        /// Initializes a new instance of the <see cref="Context" /> class.
        /// </summary>
        /// <param name="configuration">The configuration object.</param>
        /// <param name="serializer">The serializer.</param>
        public RedisContext(ConfigurationOptions configuration, ISerializer serializer) : this(configuration, serializer, null) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="Context" /> class injecting the connection multiplexer and serializer to use.
        /// </summary>
        /// <param name="connection">The connection multiplexer to use.</param>
        /// <param name="serializer">The serializer.</param>
        public RedisContext(IConnectionMultiplexer connection, ISerializer serializer)
        {
            _internalContext = new RedisProviderContext(connection, serializer);
            _cacheProvider = new RedisCacheProvider(_internalContext);
            _collectionProvider = new RedisCollectionProvider(_internalContext, _cacheProvider);
            _geoProvider = new RedisGeoProvider(_internalContext, _cacheProvider);
            _pubsubProvider = new RedisPubSubProvider(_internalContext);
            _keyEventsProvider = new RedisKeyEventsProvider(_internalContext);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Context" /> class injecting the connection multiplexer to use.
        /// </summary>
        /// <param name="connection">The connection multiplexer to use.</param>
        /// <param name="serializer">The serializer.</param>
        public RedisContext(IConnectionMultiplexer connection)
        {
            _internalContext = new RedisProviderContext(connection, new JsonSerializer());
            _cacheProvider = new RedisCacheProvider(_internalContext);
            _collectionProvider = new RedisCollectionProvider(_internalContext, _cacheProvider);
            _geoProvider = new RedisGeoProvider(_internalContext, _cacheProvider);
            _pubsubProvider = new RedisPubSubProvider(_internalContext);
            _keyEventsProvider = new RedisKeyEventsProvider(_internalContext);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Context" /> class.
        /// </summary>
        /// <param name="configuration">The configuration string.</param>
        /// <param name="serializer">The serializer.</param>
        /// <param name="log">The textwriter to use for logging purposes.</param>
        public RedisContext(string configuration, ISerializer serializer, TextWriter log)
        {
            _internalContext = new RedisProviderContext(configuration, serializer, log);
            _cacheProvider = new RedisCacheProvider(_internalContext);
            _collectionProvider = new RedisCollectionProvider(_internalContext, _cacheProvider);
            _geoProvider = new RedisGeoProvider(_internalContext, _cacheProvider);
            _pubsubProvider = new RedisPubSubProvider(_internalContext);
            _keyEventsProvider = new RedisKeyEventsProvider(_internalContext);
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="Context" /> class.
        /// </summary>
        /// <param name="configuration">The configuration object.</param>
        /// <param name="serializer">The serializer.</param>
        /// <param name="log">The textwriter to use for logging purposes.</param>
        public RedisContext(ConfigurationOptions configuration, ISerializer serializer, TextWriter log)
        {
            _internalContext = new RedisProviderContext(configuration, serializer, log);
            _cacheProvider = new RedisCacheProvider(_internalContext);
            _collectionProvider = new RedisCollectionProvider(_internalContext, _cacheProvider);
            _geoProvider = new RedisGeoProvider(_internalContext, _cacheProvider);
            _pubsubProvider = new RedisPubSubProvider(_internalContext);
            _keyEventsProvider = new RedisKeyEventsProvider(_internalContext);
        }
        #endregion

        #region IContext implementation
        /// <summary>
        /// Gets the cache API.
        /// </summary>
        public ICacheProvider Cache
        {
            get { return _cacheProvider; }
        }
        /// <summary>
        /// Gets the collection API.
        /// </summary>
        /// <value>The collections.</value>
        public ICollectionProvider Collections
        {
            get { return _collectionProvider; }
        }
        /// <summary>
        /// Gets the geo spatial API.
        /// </summary>
        public IGeoProvider GeoSpatial
        {
            get { return _geoProvider; }
        }
        /// <summary>
        /// Gets the pub sub API.
        /// </summary>
        public IPubSubProvider PubSub
        {
            get { return _pubsubProvider; }
        }

        /// <summary>
        /// Access the key events API.
        /// </summary>
        /// <value>The key events.</value>
        public IKeyEventsProvider KeyEvents
        {
            get { return _keyEventsProvider; }
        }

        #endregion

        #region Public methods
        /// <summary>
        /// Gets the StackExchange.Redis's connection multiplexer.
        /// Use this if you want to directly access the SE.Redis API.
        /// </summary>
        /// <returns>IConnectionMultiplexer.</returns>
        public IConnectionMultiplexer GetConnectionMultiplexer()
        {
            return _internalContext.RedisConnection;
        }

        /// <summary>
        /// Gets the serializer for this context.
        /// </summary>
        public ISerializer GetSerializer()
        {
            return _internalContext.Serializer;
        }

        /// <summary>
        /// Disposes this instance.
        /// </summary>
        public void Dispose()
        {
            _internalContext.RedisConnection.Dispose();
        }
        #endregion
    }
}
