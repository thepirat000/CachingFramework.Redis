using System;
using MemoryPack;
using StackExchange.Redis;

namespace CachingFramework.Redis.MemoryPack
{
    /// <summary>
    /// Class MemoryPackSerializer.
    /// </summary>
    public class MemPackSerializer : Contracts.SerializerBase
    {
        readonly MemoryPackSerializerOptions _options;
        public MemPackSerializer(MemoryPackSerializerOptions options = null)
        {
            _options = options ?? MemoryPackSerializerOptions.Default;
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
            return MemoryPackSerializer.Serialize(value,_options);
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
            ReadOnlyMemory<byte> rom = (ReadOnlyMemory<byte>)value;
            return MemoryPackSerializer.Deserialize<T>(rom.Span, _options);
            
        }
    }
}
