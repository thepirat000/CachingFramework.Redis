using System;
using System.IO;
using System.IO.Compression;
using System.Runtime.Serialization.Formatters.Binary;
using CachingFramework.Redis.Contracts;

namespace CachingFramework.Redis.Serializers
{
    /// <summary>
    /// Binary Serializer with GZIP compression.
    /// Objects to serialize must be marked with [Serializable] attribute.
    /// </summary>
    public class BinarySerializer : ISerializer
    {
        /// <summary>
        /// The buffer size
        /// </summary>
        private const int BufferSize = 64*1024; //64kB
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
                return Compress(mStream.ToArray());
            }
        }
        /// <summary>
        /// Deserializes the specified value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>System.Object.</returns>
        public object Deserialize(byte[] value)
        {
            using (var memoryStream = new MemoryStream(Decompress(value)))
            {
                return _formatter.Deserialize(memoryStream);
            }
        }
        /// <summary>
        /// Compresses the specified input data.
        /// </summary>
        /// <param name="inputData">The input data.</param>
        public static byte[] Compress(byte[] inputData)
        {
            using (var compressIntoMs = new MemoryStream())
            {
                using (var gzs = new BufferedStream(new GZipStream(compressIntoMs, CompressionMode.Compress), BufferSize))
                {
                    gzs.Write(inputData, 0, inputData.Length);
                }
                return compressIntoMs.ToArray();
            }
        }
        /// <summary>
        /// Decompresses the specified input data.
        /// </summary>
        /// <param name="inputData">The input data.</param>
        public static byte[] Decompress(byte[] inputData)
        {
            using (var compressedMs = new MemoryStream(inputData))
            {
                using (var decompressedMs = new MemoryStream())
                {
                    using (var gzs = new BufferedStream(new GZipStream(compressedMs, CompressionMode.Decompress), BufferSize))
                    {
                        gzs.CopyTo(decompressedMs);
                    }
                    return decompressedMs.ToArray();
                }
            }
        }
    }
}