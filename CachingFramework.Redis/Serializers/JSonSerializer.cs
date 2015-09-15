using CachingFramework.Redis.Contracts;
using Newtonsoft.Json;

namespace CachingFramework.Redis.Serializers
{
    /// <summary>
    /// JSon Serializer implementation of ISerializer.
    /// </summary>
    internal class JSonSerializer : ISerializer
    {
        /// <summary>
        /// Serializes the specified value.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value">The value.</param>
        /// <returns>System.String.</returns>
        public string Serialize<T>(T value)
        {
            return Serialize((object)value);
        }
        /// <summary>
        /// Deserializes the specified value.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value">The value.</param>
        /// <returns>``0.</returns>
        public T Deserialize<T>(string value)
        {
            var deserialized = JsonConvert.DeserializeObject<T>(value);
            return deserialized;
        }
        /// <summary>
        /// Serializes the specified value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>System.String.</returns>
        public string Serialize(object value)
        {
            var config = new JsonSerializerSettings { PreserveReferencesHandling = PreserveReferencesHandling.Objects };
            var serialized = JsonConvert.SerializeObject(value, config);
            return serialized;
        }
        /// <summary>
        /// Deserializes the specified value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>System.Object.</returns>
        public object Deserialize(string value)
        {
            var deserialized = JsonConvert.DeserializeObject(value);
            return deserialized;
        }
    }
}
