using CachingFramework.Redis.Contracts;
using StackExchange.Redis;

namespace CachingFramework.Redis.Providers
{
    /// <summary>
    /// Redis Provider Base Class.
    /// </summary>
    internal abstract class RedisProviderBase
    {
        #region Fields
        /// <summary>
        /// The redis provider context
        /// </summary>
        internal protected readonly RedisProviderContext _context;
        #endregion  

        #region Properties
        /// <summary>
        /// Gets the redis connection.
        /// </summary>
        /// <value>The redis connection.</value>
        internal protected ConnectionMultiplexer RedisConnection { get { return _context.RedisConnection; } }
        /// <summary>
        /// Gets the serializer.
        /// </summary>
        /// <value>The serializer.</value>
        protected ISerializer Serializer { get { return _context.Serializer; } }
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="RedisProviderBase"/> class.
        /// </summary>
        /// <param name="context">The context.</param>
        internal RedisProviderBase(RedisProviderContext context)
        {
            _context = context;
        }
        #endregion
    }
}