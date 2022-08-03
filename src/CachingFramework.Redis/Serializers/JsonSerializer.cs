using System.Text.Json;
using StackExchange.Redis;

namespace CachingFramework.Redis.Serializers
{
    /// <summary>
    /// Class JsonSerializer.
    /// </summary>
    public class JsonSerializer : Contracts.SerializerBase
    {
        /// <summary>
        /// The _settings
        /// </summary>
        private readonly JsonSerializerOptions _settings;

        /// <summary>
        /// Initializes a new instance of the <see cref="JsonSerializer"/> class.
        /// </summary>
        public JsonSerializer()
        {
            _settings = new JsonSerializerOptions()
            {
                PropertyNameCaseInsensitive = true
            };
            _settings.Converters.Add(new HandleSpecialDoublesAsStrings());
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="JsonSerializer"/> class.
        /// </summary>
        /// <param name="settings">The settings.</param>
        public JsonSerializer(JsonSerializerOptions settings)
        {
            _settings = settings;
        }

        /// <summary>
        /// Serializes the specified value.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value">The value.</param>
        /// <returns>System.Byte[].</returns>
        public override RedisValue Serialize<T>(T value)
        {
            return System.Text.Json.JsonSerializer.Serialize(value, _settings);
        }

        /// <summary>
        /// Deserializes the specified value.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value">The value.</param>
        /// <returns>T.</returns>
        public override T Deserialize<T>(RedisValue value)
        {
            return System.Text.Json.JsonSerializer.Deserialize<T>(value, _settings);
        }
    }
}
