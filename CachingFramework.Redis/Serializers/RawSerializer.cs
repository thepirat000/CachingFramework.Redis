using System;
using System.Collections.Generic;
using System.Globalization;
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
        private const string DateFormat = "yyyyMMddHHmmssfffffff";
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
        /// <summary>
        /// The default cultureinfo to use in the ToString methods.
        /// </summary>
        private static readonly CultureInfo Culture = CultureInfo.InvariantCulture;
        #endregion
        
        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="RawSerializer"/> class.
        /// </summary>
        public RawSerializer()
        {
            _serialDict = new Dictionary<Type, Func<object, byte[]>>
            {
                [typeof (String)]   = o => GetBytes(o.ToString()), 
                [typeof (Char)]     = o => GetBytes(o.ToString()),
                [typeof (Boolean)]  = o => GetBytes((bool)o ? "1" : "0"),
                [typeof (Byte)]     = o => GetBytes(o.ToString()),
                [typeof (SByte)]    = o => GetBytes(o.ToString()),
                [typeof (Int16)]    = o => GetBytes(o.ToString()),
                [typeof (Int32)]    = o => GetBytes(o.ToString()),
                [typeof (Int64)]    = o => GetBytes(o.ToString()),
                [typeof (UInt16)]   = o => GetBytes(o.ToString()),
                [typeof (UInt32)]   = o => GetBytes(o.ToString()),
                [typeof (UInt64)]   = o => GetBytes(o.ToString()),
                [typeof (IntPtr)]   = o => GetBytes(o.ToString()),
                [typeof (UIntPtr)]  = o => GetBytes(o.ToString()),
                [typeof (Double)]   = o => GetBytes(((Double)o).ToString(FloatFormat, Culture)),
                [typeof (Single)]   = o => GetBytes(((Single)o).ToString(FloatFormat, Culture)),
                [typeof (Decimal)]  = o => GetBytes(((Decimal)o).ToString(FloatFormat, Culture)),
                [typeof (DateTime)] = o => GetBytes(((DateTime)o).ToString(DateFormat, Culture)),
                [typeof (object)]   = o => base.Serialize(o)
            };
            _deserialDict = new Dictionary<Type, Func<byte[], object>>
            {
                [typeof (String)]   = b => GetString(b), 
                [typeof (Char)]     = b => Convert.ToChar(GetString(b)),
                [typeof (Boolean)]  = b => GetString(b) == "1",
                [typeof (Byte)]     = b => Byte.Parse(GetString(b), Culture),
                [typeof (SByte)]    = b => SByte.Parse(GetString(b), Culture),
                [typeof (Int16)]    = b => Int16.Parse(GetString(b), Culture),
                [typeof (Int32)]    = b => Int32.Parse(GetString(b), Culture),
                [typeof (Int64)]    = b => Int64.Parse(GetString(b), Culture),
                [typeof (UInt16)]   = b => UInt16.Parse(GetString(b), Culture),
                [typeof (UInt32)]   = b => UInt32.Parse(GetString(b), Culture),
                [typeof (UInt64)]   = b => UInt64.Parse(GetString(b), Culture),
                [typeof (IntPtr)]   = b => new IntPtr(long.Parse(GetString(b), Culture)),
                [typeof (UIntPtr)]  = b => new UIntPtr(ulong.Parse(GetString(b), Culture)),
                [typeof (Double)]   = b => Double.Parse(GetString(b), Culture),
                [typeof (Single)]   = b => Single.Parse(GetString(b), Culture),
                [typeof (Decimal)]  = b => Decimal.Parse(GetString(b), Culture),
                [typeof (DateTime)] = b => DateTime.ParseExact(GetString(b), DateFormat, Culture),
                [typeof (object)]   = b => base.Deserialize(b)
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

        #region Private Methods
        /// <summary>
        /// Get a byte array to encode the given string in UTF8
        /// </summary>
        private static byte[] GetBytes(string s)
        {
            return Encoding.UTF8.GetBytes(s);
        }
        /// <summary>
        /// Get a string from the UTF8 encoded byte array
        /// </summary>
        private static string GetString(byte[] b)
        {
            return Encoding.UTF8.GetString(b);
        }
        #endregion
    }
}
