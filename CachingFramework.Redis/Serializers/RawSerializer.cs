using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using CachingFramework.Redis.Contracts;

namespace CachingFramework.Redis.Serializers
{
    /// <summary>
    /// Primitive types, strings and datetimes are encoded as a UTF-8 string.
    /// Any other types are Binary Serialized with GZIP compression.
    /// Objects to serialize must be marked with [Serializable] attribute.
    /// </summary>
    public class RawSerializer : BinarySerializer, ISerializer
    {
        #region Fields
        /// <summary>
        /// The date format
        /// </summary>
        private const string DateFormat = "yyyyMMddHHmmssfff";
        /// <summary>
        /// The float format
        /// </summary>
        private const string FloatFormat = "F"; 
        /// <summary>
        /// Dictionary of serializer methods per type
        /// </summary>
        private readonly Dictionary<Type, Func<object, byte[]>> _serialDict;
        /// <summary>
        /// Dictionary of deserializer methods per type
        /// </summary>
        private readonly Dictionary<Type, Func<byte[], object>> _deserialDict;
        #endregion
        
        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="RawSerializer"/> class.
        /// </summary>
        public RawSerializer()
        {
            Func<object, byte[]> toString = o => Encoding.UTF8.GetBytes(o.ToString());
            var ic = CultureInfo.InvariantCulture;
            _serialDict = new Dictionary<Type, Func<object, byte[]>>
            {
                {typeof (String), toString}, 
                {typeof (Char), toString},
                {typeof (Boolean), o => Encoding.UTF8.GetBytes(((bool)o).ToString(ic))},
                {typeof (Byte), toString},
                {typeof (SByte), toString},
                {typeof (Int16), toString},
                {typeof (Int32), toString},
                {typeof (Int64), toString},
                {typeof (UInt16), toString},
                {typeof (UInt32), toString},
                {typeof (UInt64), toString},
                {typeof (IntPtr), toString},
                {typeof (UIntPtr), toString},
                {typeof (Double), o => Encoding.UTF8.GetBytes(((Double)o).ToString(FloatFormat, ic))},
                {typeof (Single), o => Encoding.UTF8.GetBytes(((Single)o).ToString(FloatFormat, ic))},
                {typeof (Decimal), o => Encoding.UTF8.GetBytes(((Decimal)o).ToString(FloatFormat, ic))},
                {typeof (DateTime), o => Encoding.UTF8.GetBytes(((DateTime)o).ToString(DateFormat, ic))},
                {typeof (object), o => base.Serialize(o)}
            };
            _deserialDict = new Dictionary<Type, Func<byte[], object>>
            {
                {typeof (String), b => Encoding.UTF8.GetString(b)}, 
                {typeof (Char), b => Convert.ToChar(Encoding.UTF8.GetString(b))},
                {typeof (Boolean), b => bool.Parse(Encoding.UTF8.GetString(b))},
                {typeof (Byte), b => Byte.Parse(Encoding.UTF8.GetString(b), ic)},
                {typeof (SByte), b => SByte.Parse(Encoding.UTF8.GetString(b), ic)},
                {typeof (Int16), b => Int16.Parse(Encoding.UTF8.GetString(b), ic)},
                {typeof (Int32), b => Int32.Parse(Encoding.UTF8.GetString(b), ic)},
                {typeof (Int64), b => Int64.Parse(Encoding.UTF8.GetString(b), ic)},
                {typeof (UInt16), b => UInt16.Parse(Encoding.UTF8.GetString(b), ic)},
                {typeof (UInt32), b => UInt32.Parse(Encoding.UTF8.GetString(b), ic)},
                {typeof (UInt64), b => UInt64.Parse(Encoding.UTF8.GetString(b), ic)},
                {typeof (IntPtr), b => new IntPtr(long.Parse(Encoding.UTF8.GetString(b), ic))},
                {typeof (UIntPtr), b => new UIntPtr(ulong.Parse(Encoding.UTF8.GetString(b), ic))},
                {typeof (Double), b => Double.Parse(Encoding.UTF8.GetString(b), ic)},
                {typeof (Single), b => Single.Parse(Encoding.UTF8.GetString(b), ic)},
                {typeof (Decimal), b => Decimal.Parse(Encoding.UTF8.GetString(b), ic)},
                {typeof (DateTime), b => DateTime.ParseExact(Encoding.UTF8.GetString(b), DateFormat, ic)},
                {typeof (object), b => base.Deserialize(b)}
            };
        }
        #endregion

        #region Public methods
        /// <summary>
        /// Override the serialization/deserialization method for a given type.
        /// </summary>
        /// <param name="serializeMethod">The serialize method.</param>
        /// <param name="deserializeMethod">The deserialize method.</param>
        public void SetSerializerFor<T>(Func<T, byte[]> serializeMethod,
            Func<byte[], T> deserializeMethod)
        {
            _serialDict[typeof(T)] = o => serializeMethod((T)o);
            _deserialDict[typeof(T)] = b => deserializeMethod(b);
        }
        #endregion

        #region ISerializer implementation

        /// <summary>
        /// Serializes the specified value.
        /// </summary>
        public override byte[] Serialize<T>(T value)
        {
            var type = value.GetType();
            return _serialDict.ContainsKey(type) 
                ? _serialDict[type](value)
                : _serialDict[typeof(object)](value);
        }
        /// <summary>
        /// Deserializes the specified value.
        /// </summary>
        public override T Deserialize<T>(byte[] value)
        {
            var type = typeof(T);
            return (T) (_deserialDict.ContainsKey(type)
                ? _deserialDict[type](value)
                : _deserialDict[typeof (object)](value));
        }
        #endregion
    }
}
