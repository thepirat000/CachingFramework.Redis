using System;

namespace CachingFramework.Redis.Contracts
{
    /// <summary>
    /// Enum BitfieldType
    /// </summary>
    public enum BitfieldType
    {
        /// <summary>Unsigned integer of 1 bit. (0 to 1).</summary>
        u1,
        /// <summary>Unsigned integer of 2 bits. (0 to 3).</summary>
        u2,
        /// <summary>Unsigned integer of 3 bits. (0 to 7).</summary>
        u3,
        /// <summary>Unsigned integer of 4 bits. (0 to 15).</summary>
        u4,
        /// <summary>Unsigned integer of 5 bits. (0 to 31).</summary>
        u5,
        /// <summary>Unsigned integer of 6 bits. (0 to 63).</summary>
        u6,
        /// <summary>Unsigned integer of 7 bits. (0 to 127).</summary>
        u7,
        /// <summary>Unsigned integer of 8 bits. (0 to 255). (System.Byte .NET equivalent)</summary>
        u8,
        /// <summary>Unsigned integer of 9 bits. (0 to 511).</summary>
        u9,
        /// <summary>Unsigned integer of 10 bits. (0 to 1023).</summary>
        u10,
        /// <summary>Unsigned integer of 11 bits. (0 to 2047).</summary>
        u11,
        /// <summary>Unsigned integer of 12 bits. (0 to 4095).</summary>
        u12,
        /// <summary>Unsigned integer of 13 bits. (0 to 8191).</summary>
        u13,
        /// <summary>Unsigned integer of 14 bits. (0 to 16383).</summary>
        u14,
        /// <summary>Unsigned integer of 15 bits. (0 to 32767).</summary>
        u15,
        /// <summary>Unsigned integer of 16 bits. (0 to 65535). (System.UInt16 .NET equivalent)</summary>
        u16,
        /// <summary>Unsigned integer of 17 bits. (0 to 2^17-1).</summary>
        u17,
        /// <summary>Unsigned integer of 18 bits. (0 to 2^18-1).</summary>
        u18,
        /// <summary>Unsigned integer of 19 bits. (0 to 2^19-1).</summary>
        u19,
        /// <summary>Unsigned integer of 20 bits. (0 to 2^20-1).</summary>
        u20,
        /// <summary>Unsigned integer of 21 bits. (0 to 2^21-1).</summary>
        u21,
        /// <summary>Unsigned integer of 22 bits. (0 to 2^22-1).</summary>
        u22,
        /// <summary>Unsigned integer of 23 bits. (0 to 2^23-1).</summary>
        u23,
        /// <summary>Unsigned integer of 24 bits. (0 to 2^24-1).</summary>
        u24,
        /// <summary>Unsigned integer of 25 bits. (0 to 2^25-1).</summary>
        u25,
        /// <summary>Unsigned integer of 26 bits. (0 to 2^26-1).</summary>
        u26,
        /// <summary>Unsigned integer of 27 bits. (0 to 2^27-1).</summary>
        u27,
        /// <summary>Unsigned integer of 28 bits. (0 to 2^28-1).</summary>
        u28,
        /// <summary>Unsigned integer of 29 bits. (0 to 2^29-1).</summary>
        u29,
        /// <summary>Unsigned integer of 30 bits. (0 to 2^30-1).</summary>
        u30,
        /// <summary>Unsigned integer of 31 bits. (0 to 2^31-1).</summary>
        u31,
        /// <summary>Unsigned integer of 32 bits. (0 to 2^32-1). (System.UInt32 .NET equivalent)</summary>
        u32,
        /// <summary>Unsigned integer of 33 bits. (0 to 2^33-1).</summary>
        u33,
        /// <summary>Unsigned integer of 34 bits. (0 to 2^34-1).</summary>
        u34,
        /// <summary>Unsigned integer of 35 bits. (0 to 2^35-1).</summary>
        u35,
        /// <summary>Unsigned integer of 36 bits. (0 to 2^36-1).</summary>
        u36,
        /// <summary>Unsigned integer of 37 bits. (0 to 2^37-1).</summary>
        u37,
        /// <summary>Unsigned integer of 38 bits. (0 to 2^38-1).</summary>
        u38,
        /// <summary>Unsigned integer of 39 bits. (0 to 2^39-1).</summary>
        u39,
        /// <summary>Unsigned integer of 40 bits. (0 to 2^40-1).</summary>
        u40,
        /// <summary>Unsigned integer of 41 bits. (0 to 2^41-1).</summary>
        u41,
        /// <summary>Unsigned integer of 42 bits. (0 to 2^42-1).</summary>
        u42,
        /// <summary>Unsigned integer of 43 bits. (0 to 2^43-1).</summary>
        u43,
        /// <summary>Unsigned integer of 44 bits. (0 to 2^44-1).</summary>
        u44,
        /// <summary>Unsigned integer of 45 bits. (0 to 2^45-1).</summary>
        u45,
        /// <summary>Unsigned integer of 46 bits. (0 to 2^46-1).</summary>
        u46,
        /// <summary>Unsigned integer of 47 bits. (0 to 2^47-1).</summary>
        u47,
        /// <summary>Unsigned integer of 48 bits. (0 to 2^48-1).</summary>
        u48,
        /// <summary>Unsigned integer of 49 bits. (0 to 2^49-1).</summary>
        u49,
        /// <summary>Unsigned integer of 50 bits. (0 to 2^50-1).</summary>
        u50,
        /// <summary>Unsigned integer of 51 bits. (0 to 2^51-1).</summary>
        u51,
        /// <summary>Unsigned integer of 52 bits. (0 to 2^52-1).</summary>
        u52,
        /// <summary>Unsigned integer of 53 bits. (0 to 2^53-1).</summary>
        u53,
        /// <summary>Unsigned integer of 54 bits. (0 to 2^54-1).</summary>
        u54,
        /// <summary>Unsigned integer of 55 bits. (0 to 2^55-1).</summary>
        u55,
        /// <summary>Unsigned integer of 56 bits. (0 to 2^56-1).</summary>
        u56,
        /// <summary>Unsigned integer of 57 bits. (0 to 2^57-1).</summary>
        u57,
        /// <summary>Unsigned integer of 58 bits. (0 to 2^58-1).</summary>
        u58,
        /// <summary>Unsigned integer of 59 bits. (0 to 2^59-1).</summary>
        u59,
        /// <summary>Unsigned integer of 60 bits. (0 to 2^60-1).</summary>
        u60,
        /// <summary>Unsigned integer of 61 bits. (0 to 2^61-1).</summary>
        u61,
        /// <summary>Unsigned integer of 62 bits. (0 to 2^62-1).</summary>
        u62,
        /// <summary>Unsigned integer of 63 bits. (0 to 2^63-1).</summary>
        u63,
        /// <summary>Signed integer of 1 bit. (-1 to 0).</summary>
        i1,
        /// <summary>Signed integer of 2 bits. (-2 to 1).</summary>
        i2,
        /// <summary>Signed integer of 3 bits. (-4 to 3).</summary>
        i3,
        /// <summary>Signed integer of 4 bits. (-8 to 7).</summary>
        i4,
        /// <summary>Signed integer of 5 bits. (-16 to 15).</summary>
        i5,
        /// <summary>Signed integer of 6 bits. (-32 to 31).</summary>
        i6,
        /// <summary>Signed integer of 7 bits. (-64 to 63).</summary>
        i7,
        /// <summary>Signed integer of 8 bits. (-128 to 127). (System.SByte .NET equivalent)</summary>
        i8,
        /// <summary>Signed integer of 9 bits. (-256 to 255).</summary>
        i9,
        /// <summary>Signed integer of 10 bits. (-512 to 511).</summary>
        i10,
        /// <summary>Signed integer of 11 bits. (-1024 to 1023).</summary>
        i11,
        /// <summary>Signed integer of 12 bits. (-2048 to 2047).</summary>
        i12,
        /// <summary>Signed integer of 13 bits. (-4096 to 4095).</summary>
        i13,
        /// <summary>Signed integer of 14 bits. (-8192 to 8191).</summary>
        i14,
        /// <summary>Signed integer of 15 bits. (-16384 to 16383).</summary>
        i15,
        /// <summary>Signed integer of 16 bits. (-32768 to 32767). (System.Int16 .NET equivalent)</summary>
        i16,
        /// <summary>Signed integer of 17 bits. (-65536 to 65535).</summary>
        i17,
        /// <summary>Signed integer of 18 bits. (-2^18/2 to 2^18/2-1).</summary>
        i18,
        /// <summary>Signed integer of 19 bits. (-2^19/2 to 2^19/2-1).</summary>
        i19,
        /// <summary>Signed integer of 20 bits. (-2^20/2 to 2^20/2-1).</summary>
        i20,
        /// <summary>Signed integer of 21 bits. (-2^21/2 to 2^21/2-1).</summary>
        i21,
        /// <summary>Signed integer of 22 bits. (-2^22/2 to 2^22/2-1).</summary>
        i22,
        /// <summary>Signed integer of 23 bits. (-2^23/2 to 2^23/2-1).</summary>
        i23,
        /// <summary>Signed integer of 24 bits. (-2^24/2 to 2^24/2-1).</summary>
        i24,
        /// <summary>Signed integer of 25 bits. (-2^25/2 to 2^25/2-1).</summary>
        i25,
        /// <summary>Signed integer of 26 bits. (-2^26/2 to 2^26/2-1).</summary>
        i26,
        /// <summary>Signed integer of 27 bits. (-2^27/2 to 2^27/2-1).</summary>
        i27,
        /// <summary>Signed integer of 28 bits. (-2^28/2 to 2^28/2-1).</summary>
        i28,
        /// <summary>Signed integer of 29 bits. (-2^29/2 to 2^29/2-1).</summary>
        i29,
        /// <summary>Signed integer of 30 bits. (-2^30/2 to 2^30/2-1).</summary>
        i30,
        /// <summary>Signed integer of 31 bits. (-2^31/2 to 2^31/2-1).</summary>
        i31,
        /// <summary>Signed integer of 32 bits. (-2^32/2 to 2^32/2-1). (System.Int32 .NET equivalent)</summary>
        i32,
        /// <summary>Signed integer of 33 bits. (-2^33/2 to 2^33/2-1).</summary>
        i33,
        /// <summary>Signed integer of 34 bits. (-2^34/2 to 2^34/2-1).</summary>
        i34,
        /// <summary>Signed integer of 35 bits. (-2^35/2 to 2^35/2-1).</summary>
        i35,
        /// <summary>Signed integer of 36 bits. (-2^36/2 to 2^36/2-1).</summary>
        i36,
        /// <summary>Signed integer of 37 bits. (-2^37/2 to 2^37/2-1).</summary>
        i37,
        /// <summary>Signed integer of 38 bits. (-2^38/2 to 2^38/2-1).</summary>
        i38,
        /// <summary>Signed integer of 39 bits. (-2^39/2 to 2^39/2-1).</summary>
        i39,
        /// <summary>Signed integer of 40 bits. (-2^40/2 to 2^40/2-1).</summary>
        i40,
        /// <summary>Signed integer of 41 bits. (-2^41/2 to 2^41/2-1).</summary>
        i41,
        /// <summary>Signed integer of 42 bits. (-2^42/2 to 2^42/2-1).</summary>
        i42,
        /// <summary>Signed integer of 43 bits. (-2^43/2 to 2^43/2-1).</summary>
        i43,
        /// <summary>Signed integer of 44 bits. (-2^44/2 to 2^44/2-1).</summary>
        i44,
        /// <summary>Signed integer of 45 bits. (-2^45/2 to 2^45/2-1).</summary>
        i45,
        /// <summary>Signed integer of 46 bits. (-2^46/2 to 2^46/2-1).</summary>
        i46,
        /// <summary>Signed integer of 47 bits. (-2^47/2 to 2^47/2-1).</summary>
        i47,
        /// <summary>Signed integer of 48 bits. (-2^48/2 to 2^48/2-1).</summary>
        i48,
        /// <summary>Signed integer of 49 bits. (-2^49/2 to 2^49/2-1).</summary>
        i49,
        /// <summary>Signed integer of 50 bits. (-2^50/2 to 2^50/2-1).</summary>
        i50,
        /// <summary>Signed integer of 51 bits. (-2^51/2 to 2^51/2-1).</summary>
        i51,
        /// <summary>Signed integer of 52 bits. (-2^52/2 to 2^52/2-1).</summary>
        i52,
        /// <summary>Signed integer of 53 bits. (-2^53/2 to 2^53/2-1).</summary>
        i53,
        /// <summary>Signed integer of 54 bits. (-2^54/2 to 2^54/2-1).</summary>
        i54,
        /// <summary>Signed integer of 55 bits. (-2^55/2 to 2^55/2-1).</summary>
        i55,
        /// <summary>Signed integer of 56 bits. (-2^56/2 to 2^56/2-1).</summary>
        i56,
        /// <summary>Signed integer of 57 bits. (-2^57/2 to 2^57/2-1).</summary>
        i57,
        /// <summary>Signed integer of 58 bits. (-2^58/2 to 2^58/2-1).</summary>
        i58,
        /// <summary>Signed integer of 59 bits. (-2^59/2 to 2^59/2-1).</summary>
        i59,
        /// <summary>Signed integer of 60 bits. (-2^60/2 to 2^60/2-1).</summary>
        i60,
        /// <summary>Signed integer of 61 bits. (-2^61/2 to 2^61/2-1).</summary>
        i61,
        /// <summary>Signed integer of 62 bits. (-2^62/2 to 2^62/2-1).</summary>
        i62,
        /// <summary>Signed integer of 63 bits. (-2^63/2 to 2^63/2-1).</summary>
        i63,
        /// <summary>Signed integer of 64 bits. (-2^64/2 to 2^64/2-1). (System.Int64 .NET equivalent)</summary>
        i64
    }
}