using StackExchange.Redis;
using StackExchange.Redis.KeyspaceIsolation;

namespace CachingFramework.Redis
{
    /// <summary>
    /// Custom redis database options
    /// </summary>
    public class DatabaseOptions
    {
        /// <summary>
        /// The key prefix to use for all keys. Default is null to use no prefix.
        /// </summary>
        public string KeyPrefix { get; set; } = "";
        
        /// <summary>
        /// The redis database index to use. Default is -1 to use the default database in the connection.
        /// </summary>
        public int DbIndex { get; set; } = -1;

        internal IDatabase GetDatabase(IConnectionMultiplexer multiplexer)
        {
            return multiplexer.GetDatabase(DbIndex).WithKeyPrefix(KeyPrefix ?? "");
        }
    }
}