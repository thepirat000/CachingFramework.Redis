using System;
using System.IO;
using MessagePack;
using StackExchange.Redis;

namespace CachingFramework.Redis.MsgPack
{
    /// <summary>
    /// Class MsgPackSerializer.
    /// </summary>
    public class MsgPackSerializer : Contracts.ISerializer
    {
        private readonly MessagePackSerializerOptions options;
        public MsgPackSerializer(MessagePackSerializerOptions options = null)
        {
            if (options == null)
                options = MessagePackSerializerOptions.Standard
                                .WithSecurity(MessagePackSecurity.UntrustedData);
            this.options = options;

        }
        /// <summary>
        /// Serializes the specified value.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value">The value.</param>
        /// <returns>System.Byte[].</returns>
        public RedisValue Serialize<T>(T value)
        {
            if (value == null)
            {
                return RedisValue.Null;
            }

            return MessagePackSerializer.Serialize(value, options);
        }

        /// <summary>
        /// Deserializes the specified value.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value">The value.</param>
        /// <returns>T.</returns>
        public T Deserialize<T>(RedisValue value)
        {
            if (value.IsNull)
            {
                return default;
            }
            return MessagePackSerializer.Deserialize<T>(value, options);
            
        }
    }
}
