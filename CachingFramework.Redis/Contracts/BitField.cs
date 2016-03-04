using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace CachingFramework.Redis.Contracts
{
    public sealed class BitField
    {
        /// <summary>
        /// Gets or sets the bitfield numeric type.
        /// </summary>
        private BitFieldType _type;
        private string _value;

        public BitFieldType Type { get { return _type; } }

        public BitField(Byte value)
        {
            _type = BitFieldType.u8;
            _value = value.ToString();
        }
        public BitField(SByte value)
        {
            _type = BitFieldType.i8;
            _value = value.ToString();
        }

        public BitField(Int16 value)
        {
            _type = BitFieldType.i16;
            _value = value.ToString();
        }
        public BitField(Int32 value)
        {
            _type = BitFieldType.i32;
            _value = value.ToString();
        }
        public BitField(Int64 value)
        {
            _type = BitFieldType.i64;
            _value = value.ToString();
        }
        public BitField(UInt16 value)
        {
            _type = BitFieldType.u16;
            _value = value.ToString();
        }

        public static implicit operator Int32(BitField b)
        {
            return Int32.Parse(b._value);
        }

        public static implicit operator UInt32(BitField b)
        {
            return UInt32.Parse(b._value);
        }

    }
}
