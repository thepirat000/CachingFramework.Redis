
namespace CachingFramework.Redis
{
    /// <summary>
    /// Class LuaScriptResource.
    /// </summary>
    public static class LuaScriptResource
    {
        /// <summary>
        /// The bitfield command
        /// </summary>
        public static string Bitfield = "return redis.call('bitfield', KEYS[1], unpack(ARGV))";
        /// <summary>
        /// The ZADD command
        /// </summary>
        public static string Zadd = "return redis.call('zadd', KEYS[1], unpack(ARGV))";
    }
}
