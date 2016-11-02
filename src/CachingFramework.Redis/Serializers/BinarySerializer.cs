#if (NET45 || NET461)
using System.IO;
using System.IO.Compression;
using CachingFramework.Redis.Contracts;
using System.Runtime.Serialization.Formatters.Binary;

namespace CachingFramework.Redis.Serializers
{
    /// <summary>
    /// All types are serialized using a Binary Serializer with GZIP compression.
    /// Objects to serialize must be marked with [Serializable] attribute.
    /// </summary>
    public class BinarySerializer : ISerializer
    {
        #region Fields
        /// <summary>
        /// The buffer size
        /// </summary>
        private const int BufferSize = 64*1024; //64kB
        /// <summary>
        /// The binary formatter
        /// </summary>
        private readonly BinaryFormatter _formatter = new BinaryFormatter();
        #endregion
        #region ISerializer implementation
        /// <summary>
        /// Serializes the specified value.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value">The value.</param>
        /// <returns>System.String.</returns>
        public virtual byte[] Serialize<T>(T value)
        {
            return Serialize((object)value);
        }
        /// <summary>
        /// Deserializes the specified value.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value">The value.</param>
        /// <returns>``0.</returns>
        public virtual T Deserialize<T>(byte[] value)
        {
            return (T)Deserialize(value);
        }
        #endregion
        #region Private methods
        /// <summary>
        /// Serializes the specified value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>System.String.</returns>
        protected byte[] Serialize(object value)
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
        protected object Deserialize(byte[] value)
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
        private static byte[] Compress(byte[] inputData)
        {
            using (var ms = new MemoryStream())
            {
                using (var gzs = new BufferedStream(new GZipStream(ms, CompressionMode.Compress), BufferSize))
                {
                    gzs.Write(inputData, 0, inputData.Length);
                }
                return ms.ToArray();
            }
        }
        /// <summary>
        /// Decompresses the specified input data.
        /// </summary>
        /// <param name="inputData">The input data.</param>
        private static byte[] Decompress(byte[] inputData)
        {
            using (var cMs = new MemoryStream(inputData))
            {
                using (var dMs = new MemoryStream())
                {
                    using (var gzs = new BufferedStream(new GZipStream(cMs, CompressionMode.Decompress), BufferSize))
                    {
                        gzs.CopyTo(dMs);
                    }
                    return dMs.ToArray();
                }
            }
        }
        #endregion
    }
}
#endif