
namespace CachingFramework.Redis
{
    /// <summary>
    /// Class LuaScriptResource.
    /// </summary>
    public static class LuaScriptResource
    {
        /// <summary>
        /// The geo add
        /// </summary>
        public static string GeoAdd = "return redis.call('geoadd', KEYS[1], unpack(ARGV))";
        /// <summary>
        /// The geo dist
        /// </summary>
        public static string GeoDist = "return redis.call('geodist', KEYS[1], unpack(ARGV))";
        /// <summary>
        /// The geo hash
        /// </summary>
        public static string GeoHash = "return redis.call('geohash', KEYS[1], unpack(ARGV))";
        /// <summary>
        /// The geo position
        /// </summary>
        public static string GeoPos = "return redis.call('geopos', KEYS[1], unpack(ARGV))";
        /// <summary>
        /// The geo radius
        /// </summary>
        public static string GeoRadius = "return redis.call('georadius', KEYS[1], unpack(ARGV))";
    }
}
