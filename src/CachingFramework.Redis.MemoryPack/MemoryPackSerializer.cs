using MemoryPack;

using StackExchange.Redis;

using System;

namespace CachingFramework.Redis.MemoryPack
{
    /// <summary>
    /// Class MsgPackSerializer.
    /// </summary>
    public class MemoryPackSerializer : Contracts.SerializerBase
    {
        private readonly MemoryPackSerializerOptions _options;

        public MemoryPackSerializer(MemoryPackSerializerOptions options = null)
        {
            options ??= MemoryPackSerializerOptions.Default;

            _options = options;

        }
        /// <summary>
        /// Serializes the specified value.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value">The value.</param>
        /// <returns>System.Byte[].</returns>
        public override RedisValue Serialize<T>(T value)
        {
            if (value == null)
            {
                return RedisValue.Null;
            }
            
            return global::MemoryPack.MemoryPackSerializer.Serialize(value);
        }

        /// <summary>
        /// Deserializes the specified value.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value">The value.</param>
        /// <returns>T.</returns>
        public override T Deserialize<T>(RedisValue value)
        {
            if (value.IsNull)
            {
                return default;
            }
            
            var rom = (ReadOnlyMemory<byte>)value;

            return global::MemoryPack.MemoryPackSerializer.Deserialize<T>(rom.Span, _options);
        }
    }
}
