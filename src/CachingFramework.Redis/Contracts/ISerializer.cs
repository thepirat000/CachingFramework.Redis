namespace CachingFramework.Redis.Contracts
{
    /// <summary>
    /// Interface that defines serialization/deserialization generic methods to be used by the cache engine
    /// </summary>
    public interface ISerializer
    {
        /// <summary>
        /// Serializes the specified value.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value">The value.</param>
        /// <returns>System.String.</returns>
        byte[] Serialize<T>(T value);
        /// <summary>
        /// Deserializes the specified value.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value">The value.</param>
        T Deserialize<T>(byte[] value);
    }
}
