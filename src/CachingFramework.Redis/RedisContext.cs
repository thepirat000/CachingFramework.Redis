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
        /// <summary>
        /// Gets or sets the default serializer to use when creating a new RedisContext. 
        /// </summary>
#if (NET462)
        public static ISerializer DefaultSerializer { get; set; } = new BinarySerializer();
#else
        public static ISerializer DefaultSerializer { get; set; } = new JsonSerializer();
#endif

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
        /// Initializes a new instance of the <see cref="RedisContext" /> class using Redis in localhost server default port 6379, and using the default Serializer.
        /// </summary>
        public RedisContext() : this("localhost:6379") { }
        /// <summary>
        /// Initializes a new instance of the <see cref="RedisContext" /> class given the cache engine type and its configuration string, and using the default Serializer.
        /// </summary>
        /// <param name="configuration">The configuration string.</param>
        public RedisContext(string configuration) : this(configuration, RedisContext.DefaultSerializer, (TextWriter)null) { }
        /// <summary>
        /// Initializes a new instance of the <see cref="RedisContext" /> class given the cache engine type and its configuration string, and using the default Serializer.
        /// </summary>
        /// <param name="configuration">The configuration object.</param>
        public RedisContext(ConfigurationOptions configuration) : this(configuration, RedisContext.DefaultSerializer, (TextWriter)null) { }
        /// <summary>
        /// Initializes a new instance of the <see cref="RedisContext" /> class.
        /// </summary>
        /// <param name="configuration">The configuration string.</param>
        /// <param name="serializer">The serializer.</param>
        public RedisContext(string configuration, ISerializer serializer) : this(configuration, serializer, (TextWriter)null) { }
        /// <summary>
        /// Initializes a new instance of the <see cref="RedisContext" /> class.
        /// </summary>
        /// <param name="configuration">The configuration object.</param>
        /// <param name="serializer">The serializer.</param>
        public RedisContext(ConfigurationOptions configuration, ISerializer serializer) : this(configuration, serializer, (TextWriter)null) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="RedisContext" /> class injecting the connection multiplexer and serializer to use.
        /// </summary>
        /// <param name="connection">The connection multiplexer to use.</param>
        /// <param name="serializer">The serializer.</param>
        public RedisContext(IConnectionMultiplexer connection, ISerializer serializer)
        {
            _internalContext = new RedisProviderContext(connection, serializer ?? RedisContext.DefaultSerializer);
            _cacheProvider = new RedisCacheProvider(_internalContext);
            _collectionProvider = new RedisCollectionProvider(_internalContext, _cacheProvider);
            _geoProvider = new RedisGeoProvider(_internalContext, _cacheProvider);
            _pubsubProvider = new RedisPubSubProvider(_internalContext);
            _keyEventsProvider = new RedisKeyEventsProvider(_internalContext);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RedisContext"/> class injecting the connection multiplexer to use using the default serializer.
        /// </summary>
        /// <param name="connection">The connection multiplexer to use.</param>
        public RedisContext(IConnectionMultiplexer connection)
        {
            _internalContext = new RedisProviderContext(connection, RedisContext.DefaultSerializer);
            _cacheProvider = new RedisCacheProvider(_internalContext);
            _collectionProvider = new RedisCollectionProvider(_internalContext, _cacheProvider);
            _geoProvider = new RedisGeoProvider(_internalContext, _cacheProvider);
            _pubsubProvider = new RedisPubSubProvider(_internalContext);
            _keyEventsProvider = new RedisKeyEventsProvider(_internalContext);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RedisContext" /> class.
        /// </summary>
        /// <param name="configuration">The configuration string.</param>
        /// <param name="serializer">The serializer.</param>
        /// <param name="log">The textwriter to use for logging purposes.</param>
        public RedisContext(string configuration, ISerializer serializer, TextWriter log)
        {
            _internalContext = new RedisProviderContext(configuration, serializer ?? RedisContext.DefaultSerializer, log);
            _cacheProvider = new RedisCacheProvider(_internalContext);
            _collectionProvider = new RedisCollectionProvider(_internalContext, _cacheProvider);
            _geoProvider = new RedisGeoProvider(_internalContext, _cacheProvider);
            _pubsubProvider = new RedisPubSubProvider(_internalContext);
            _keyEventsProvider = new RedisKeyEventsProvider(_internalContext);
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="RedisContext" /> class.
        /// </summary>
        /// <param name="configuration">The configuration object.</param>
        /// <param name="serializer">The serializer.</param>
        /// <param name="log">The textwriter to use for logging purposes.</param>
        public RedisContext(ConfigurationOptions configuration, ISerializer serializer, TextWriter log)
        {
            _internalContext = new RedisProviderContext(configuration, serializer ?? RedisContext.DefaultSerializer, log);
            _cacheProvider = new RedisCacheProvider(_internalContext);
            _collectionProvider = new RedisCollectionProvider(_internalContext, _cacheProvider);
            _geoProvider = new RedisGeoProvider(_internalContext, _cacheProvider);
            _pubsubProvider = new RedisPubSubProvider(_internalContext);
            _keyEventsProvider = new RedisKeyEventsProvider(_internalContext);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RedisContext" /> class given the cache engine type its configuration string and a database getter, using the default Serializer.
        /// </summary>
        /// <param name="configuration">The configuration string.</param>
        /// <param name="databaseOptions">The custom database options.</param>
        public RedisContext(string configuration, DatabaseOptions databaseOptions) : this(configuration, RedisContext.DefaultSerializer, null, databaseOptions) { }
        /// <summary>
        /// Initializes a new instance of the <see cref="RedisContext" /> class given the cache engine type its configuration options and a database getter, using the default Serializer.
        /// </summary>
        /// <param name="configuration">The configuration object.</param>
        /// <param name="databaseOptions">The custom database options.</param>
        public RedisContext(ConfigurationOptions configuration, DatabaseOptions databaseOptions) : this(configuration, RedisContext.DefaultSerializer, null, databaseOptions) { }
        /// <summary>
        /// Initializes a new instance of the <see cref="RedisContext" /> class.
        /// </summary>
        /// <param name="configuration">The configuration string.</param>
        /// <param name="serializer">The serializer.</param>
        /// <param name="databaseOptions">The custom database options.</param>
        public RedisContext(string configuration, ISerializer serializer, DatabaseOptions databaseOptions) : this(configuration, serializer, null, databaseOptions) { }
        /// <summary>
        /// Initializes a new instance of the <see cref="RedisContext" /> class.
        /// </summary>
        /// <param name="configuration">The configuration object.</param>
        /// <param name="serializer">The serializer.</param>
        /// <param name="databaseOptions">The custom database options.</param>
        public RedisContext(ConfigurationOptions configuration, ISerializer serializer, DatabaseOptions databaseOptions) : this(configuration, serializer, null, databaseOptions) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="RedisContext" /> class injecting the connection multiplexer, serializer and database getter to use.
        /// </summary>
        /// <param name="connection">The connection multiplexer to use.</param>
        /// <param name="serializer">The serializer.</param>
        /// <param name="databaseOptions">The custom database options.</param>
        public RedisContext(IConnectionMultiplexer connection, ISerializer serializer, DatabaseOptions databaseOptions)
        {
            _internalContext = new RedisProviderContext(connection, serializer ?? RedisContext.DefaultSerializer, databaseOptions);
            _cacheProvider = new RedisCacheProvider(_internalContext);
            _collectionProvider = new RedisCollectionProvider(_internalContext, _cacheProvider);
            _geoProvider = new RedisGeoProvider(_internalContext, _cacheProvider);
            _pubsubProvider = new RedisPubSubProvider(_internalContext);
            _keyEventsProvider = new RedisKeyEventsProvider(_internalContext);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RedisContext"/> class injecting the connection multiplexer to use using the default serializer.
        /// </summary>
        /// <param name="connection">The connection multiplexer to use.</param>
        /// <param name="databaseOptions">The custom database options.</param>
        public RedisContext(IConnectionMultiplexer connection, DatabaseOptions databaseOptions)
        {
            _internalContext = new RedisProviderContext(connection, RedisContext.DefaultSerializer, databaseOptions);
            _cacheProvider = new RedisCacheProvider(_internalContext);
            _collectionProvider = new RedisCollectionProvider(_internalContext, _cacheProvider);
            _geoProvider = new RedisGeoProvider(_internalContext, _cacheProvider);
            _pubsubProvider = new RedisPubSubProvider(_internalContext);
            _keyEventsProvider = new RedisKeyEventsProvider(_internalContext);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RedisContext" /> class.
        /// </summary>
        /// <param name="configuration">The configuration string.</param>
        /// <param name="serializer">The serializer.</param>
        /// <param name="log">The textwriter to use for logging purposes.</param>
        /// <param name="databaseOptions">The custom database options.</param>
        public RedisContext(string configuration, ISerializer serializer, TextWriter log, DatabaseOptions databaseOptions)
        {
            _internalContext = new RedisProviderContext(configuration, serializer ?? RedisContext.DefaultSerializer, log, databaseOptions);
            _cacheProvider = new RedisCacheProvider(_internalContext);
            _collectionProvider = new RedisCollectionProvider(_internalContext, _cacheProvider);
            _geoProvider = new RedisGeoProvider(_internalContext, _cacheProvider);
            _pubsubProvider = new RedisPubSubProvider(_internalContext);
            _keyEventsProvider = new RedisKeyEventsProvider(_internalContext);
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="RedisContext" /> class.
        /// </summary>
        /// <param name="configuration">The configuration object.</param>
        /// <param name="serializer">The serializer.</param>
        /// <param name="log">The textwriter to use for logging purposes.</param>
        /// <param name="databaseOptions">The custom database options.</param>
        public RedisContext(ConfigurationOptions configuration, ISerializer serializer, TextWriter log, DatabaseOptions databaseOptions)
        {
            _internalContext = new RedisProviderContext(configuration, serializer ?? RedisContext.DefaultSerializer, log, databaseOptions);
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
        /// Gets the Database Options for this context.
        /// </summary>
        public DatabaseOptions GetDatabaseOptions()
        {
            return _internalContext.DatabaseOptions;
        }

        /// <summary>
        /// Disposes this instance.
        /// </summary>
        public void Dispose()
        {
            _internalContext.RedisConnection.Dispose();
        }

        public override string ToString()
        {
            return _internalContext.ToString();
        }
        #endregion
    }
}
