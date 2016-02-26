using System.Text;
using Newtonsoft.Json;

namespace CachingFramework.Redis.Json
{
    /// <summary>
    /// Class JsonSerializer.
    /// </summary>
    public class JsonSerializer : Contracts.ISerializer
    {
        /// <summary>
        /// The _settings
        /// </summary>
        private readonly JsonSerializerSettings _settings;

        /// <summary>
        /// Initializes a new instance of the <see cref="JsonSerializer"/> class.
        /// </summary>
        public JsonSerializer()
            : this(new JsonSerializerSettings())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="JsonSerializer"/> class.
        /// </summary>
        /// <param name="settings">The settings.</param>
        public JsonSerializer(JsonSerializerSettings settings)
        {
            _settings = settings;
        }

        /// <summary>
        /// Serializes the specified value.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value">The value.</param>
        /// <returns>System.Byte[].</returns>
        public byte[] Serialize<T>(T value)
        {
            return Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(value, _settings));
        }

        /// <summary>
        /// Deserializes the specified value.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value">The value.</param>
        /// <returns>T.</returns>
        public T Deserialize<T>(byte[] value)
        {
            return JsonConvert.DeserializeObject<T>(Encoding.UTF8.GetString(value), _settings);
        }
    }
}
