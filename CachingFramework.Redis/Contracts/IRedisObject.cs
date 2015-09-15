using System;

namespace CachingFramework.Redis.Contracts
{
    /// <summary>
    /// Redis object interface
    /// </summary>
    public interface IRedisObject
    {
        /// <summary>
        /// Gets or sets the time to live.
        /// Null means persistent.
        /// </summary>
        TimeSpan? TimeToLive { get; set; }
    }
}
