using CachingFramework.Redis.Contracts;
using CachingFramework.Redis.Contracts.Providers;
using CachingFramework.Redis.Providers;
using CachingFramework.Redis.Serializers;

namespace CachingFramework.Redis
{
    /// <summary>
    /// Context class containing the public API.
    /// </summary>
    public class Context
    {
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
        /// <summary>
        /// Initializes a new instance of the <see cref="Context" /> class using Redis in localhost server default port 6379, and using the default BinarySerializer.
        /// </summary>
        public Context() : this("localhost:6379") {}
        /// <summary>
        /// Initializes a new instance of the <see cref="Context" /> class given the cache engine type and its configuration string, and using the default BinarySerializer.
        /// </summary>
        /// <param name="configuration">The configuration string.</param>
        public Context(string configuration) : this(configuration, new BinarySerializer()) { }
        /// <summary>
        /// Initializes a new instance of the <see cref="Context" /> class.
        /// </summary>
        /// <param name="configuration">The configuration string.</param>
        /// <param name="serializer">The serializer.</param>
        public Context(string configuration, ISerializer serializer)
        {
            var providerContext = new RedisProviderContext(configuration, serializer);
            _collectionProvider = new RedisCollectionProvider(providerContext);
            _cacheProvider = new RedisCacheProvider(providerContext);
            _geoProvider = new RedisGeoProvider(providerContext);
            _pubsubProvider = new RedisPubSubProvider(providerContext);
            
        }
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
    }
}
