using System.IO;
using System.IO.Compression;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using CachingFramework.Redis.Contracts;

namespace CachingFramework.Redis.UnitTest
{
    /// <summary>
    /// Binary Serializer with GZIP compression with the exception of strings that are encoded UTF-8 and not compressed.
    /// Objects to serialize must be marked with [Serializable] attribute.
    /// </summary>
    public class BinaryStringSerializer : ISerializer
    {
        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="BinaryStringSerializer"/> class.
        /// </summary>
        /// <param name="compressionEnabled">if set to <c>true</c> the binary serialized items will be GZIP compressed.</param>
        public BinaryStringSerializer(bool compressionEnabled = true)
        {
            _compressionEnabled = compressionEnabled;
        }
        #endregion
        #region Fields
        /// <summary>
        /// To indicate if the compression es enabled for binary serialization
        /// </summary>
        private readonly bool _compressionEnabled;
        /// <summary>
        /// The buffer size
        /// </summary>
        private const int BufferSize = 64 * 1024; //64kB
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
            return (T)Deserialize(value);
        }
        #endregion
        #region Private methods
        /// <summary>
        /// Serializes the specified value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>System.String.</returns>
        private byte[] Serialize(object value)
        {
            if (value is string)
            {
                return Encoding.UTF8.GetBytes(value as string);
            }
            using (var mStream = new MemoryStream())
            {
                _formatter.Serialize(mStream, value);
                var serialized = mStream.ToArray();
                return _compressionEnabled ? Compress(serialized) : serialized;
            }
        }
        /// <summary>
        /// Deserializes the specified value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>System.Object.</returns>
        private object Deserialize(byte[] value)
        {
            if ((_compressionEnabled && value.Length > 1 && value[0] == 0x1F && value[1] == 0x8B)                   // GZip magic number
                ||
                (!_compressionEnabled && value.Length > 1 && value[0] == 0x00 && value[value.Length - 1] == 0x0B))  // Binary serialization "magic" number
            {
                // It's a Binary serialized array
                using (var memoryStream = new MemoryStream(_compressionEnabled ? Decompress(value) : value))
                {
                    return _formatter.Deserialize(memoryStream);
                }
            }
            // It's a UTF-8 encoded string
            return Encoding.UTF8.GetString(value);
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
        private byte[] Decompress(byte[] inputData)
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