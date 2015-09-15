using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using CachingFramework.Redis.Contracts;

namespace CachingFramework.Redis.Serializers
{
    /// <summary>
    /// Binary Serializer implementation of ISerializer.
    /// Objects to serialize must be marked with [Serializable] attribute.
    /// </summary>
    public class BinarySerializer : ISerializer
    {
        /// <summary>
        /// The binary formatter
        /// </summary>
        private readonly BinaryFormatter _formatter = new BinaryFormatter();
        /// <summary>
        /// Serializes the specified value.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value">The value.</param>
        /// <returns>System.String.</returns>
        public byte[] Serialize<T>(T value)
        {
            return Serialize((object)value);
        }
        /// <summary>
        /// Deserializes the specified value.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value">The value.</param>
        /// <returns>``0.</returns>
        public T Deserialize<T>(byte[] value)
        {
            return (T) Deserialize(value);
        }
        /// <summary>
        /// Serializes the specified value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>System.String.</returns>
        public byte[] Serialize(object value)
        {
            using (var mStream = new MemoryStream())
            {
                _formatter.Serialize(mStream, value);
                return mStream.ToArray();
            }
        }
        /// <summary>
        /// Deserializes the specified value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>System.Object.</returns>
        public object Deserialize(byte[] value)
        {
            using (var memoryStream = new MemoryStream(value))
            {
                return _formatter.Deserialize(memoryStream);
            }
        }
    }
}