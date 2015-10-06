using System;

namespace CachingFramework.Redis.Contracts.RedisObjects
{
    /// <summary>
    /// Redis object interface
    /// </summary>
    public interface IRedisObject
    {
        /// <summary>
        /// Gets or sets the time to live.
        /// Null means the key is persistent.
        /// </summary>
        TimeSpan? TimeToLive { get; set; }
        /// <summary>
        /// Gets or sets the Expiration as a local datetime.
        /// Null means the key is persistent.
        /// </summary>
        DateTime? Expiration { get; set; }
    }
}
