using System;
using System.IO;
using MsgPack.Serialization;
using StackExchange.Redis;

namespace CachingFramework.Redis.MsgPack
{
    /// <summary>
    /// Class MsgPackSerializer.
    /// </summary>
    public class MsgPackSerializer : Contracts.ISerializer
    {
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
            var serializer = SerializationContext.Default.GetSerializer(value.GetType());
            using (var stream = new MemoryStream())
            {
                serializer.Pack(stream, value);
                return stream.ToArray();
            }
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
                return default(T);
            }
            using (var stream = new MemoryStream(value))
            {
                var deserialized = SerializationContext.Default.GetSerializer(typeof(T)).Unpack(stream);
                return (T)deserialized;
            }
        }
    }
}
