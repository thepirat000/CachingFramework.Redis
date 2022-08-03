using StackExchange.Redis;

namespace CachingFramework.Redis.Contracts
{
    public abstract class SerializerBase : ISerializer
    {
        /// <summary>
        /// Serializes the specified value.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value">The value.</param>
        /// <returns>System.String.</returns>
        public abstract RedisValue Serialize<T>(T value);
        /// <summary>
        /// Deserializes the specified value.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value">The value.</param>
        public abstract T Deserialize<T>(RedisValue value);
        /// <summary>
        /// The prefix for the keys that represents tags. Default value is ":$_tag_$:".
        /// </summary>
        public virtual string TagPrefix { get; set; } = ":$_tag_$:";
        /// <summary>
        /// The postfix for the keys that represents tags. Default value is null.
        /// </summary>
        public virtual string TagPostfix { get; set; } = null;
    }
}