using System.IO;
using CachingFramework.Redis.Contracts;
using CachingFramework.Redis.Contracts.Providers;
using CachingFramework.Redis.Providers;
using CachingFramework.Redis.Serializers;
using StackExchange.Redis;

namespace CachingFramework.Redis
{
    /// <summary>
    /// Context class containing the public APIs.
    /// </summary>
    public class Context : IContext
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
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="Context" /> class using Redis in localhost server default port 6379, and using the default BinarySerializer.
        /// </summary>
        public Context() : this("localhost:6379") {}
        /// <summary>
        /// Initializes a new instance of the <see cref="Context" /> class given the cache engine type and its configuration string, and using the default BinarySerializer.
        /// </summary>
        /// <param name="configuration">The configuration string.</param>
        public Context(string configuration) : this(configuration, new BinarySerializer(), null) { }
        /// <summary>
        /// Initializes a new instance of the <see cref="Context" /> class.
        /// </summary>
        /// <param name="configuration">The configuration string.</param>
        /// <param name="serializer">The serializer.</param>
        public Context(string configuration, ISerializer serializer) : this(configuration, serializer, null) { }
        /// <summary>
        /// Initializes a new instance of the <see cref="Context" /> class.
        /// </summary>
        /// <param name="configuration">The configuration string.</param>
        /// <param name="serializer">The serializer.</param>
        /// <param name="log">The textwriter to use for logging purposes.</param>
        public Context(string configuration, ISerializer serializer, TextWriter log)
        {
            _internalContext = new RedisProviderContext(configuration, serializer, log);
            _collectionProvider = new RedisCollectionProvider(_internalContext);
            _cacheProvider = new RedisCacheProvider(_internalContext);
            _geoProvider = new RedisGeoProvider(_internalContext);
            _pubsubProvider = new RedisPubSubProvider(_internalContext);
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
        #endregion
    }
}
