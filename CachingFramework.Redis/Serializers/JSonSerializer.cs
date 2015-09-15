using CachingFramework.Redis.Contracts;
using Newtonsoft.Json;

namespace CachingFramework.Redis.Serializers
{
    /// <summary>
    /// JSon Serializer implementation of ISerializer.
    /// </summary>
    public class JSonSerializer : ISerializer
    {
        /// <summary>
        /// The JSon serializer settings
        /// </summary>
        private readonly JsonSerializerSettings _settings = new JsonSerializerSettings { PreserveReferencesHandling = PreserveReferencesHandling.Objects };
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
            var deserialized = JsonConvert.DeserializeObject<T>(GetString(value));
            return deserialized;
        }
        /// <summary>
        /// Serializes the specified value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>System.String.</returns>
        public byte[] Serialize(object value)
        {
            var serialized = GetBytes(JsonConvert.SerializeObject(value, _settings));
            return serialized;
        }
        /// <summary>
        /// Deserializes the specified value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>System.Object.</returns>
        public object Deserialize(byte[] value)
        {
            var deserialized = JsonConvert.DeserializeObject(GetString(value));
            return deserialized;
        }
        /// <summary>
        /// Gets a byte array from a string.
        /// </summary>
        private static byte[] GetBytes(string str)
        {
            byte[] bytes = new byte[str.Length * sizeof(char)];
            System.Buffer.BlockCopy(str.ToCharArray(), 0, bytes, 0, bytes.Length);
            return bytes;
        }
        /// <summary>
        /// Gets a string from an array of bytes.
        /// </summary>
        private static string GetString(byte[] bytes)
        {
            char[] chars = new char[bytes.Length / sizeof(char)];
            System.Buffer.BlockCopy(bytes, 0, chars, 0, bytes.Length);
            return new string(chars);
        }
    }
}
