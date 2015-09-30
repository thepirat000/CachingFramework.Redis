using System;
using System.Reflection;

namespace CachingFramework.Redis.Contracts
{
    /// <summary>
    /// Attribute to indicate the unit abbreviation in redis
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, Inherited = false, AllowMultiple = true)]
    internal sealed class UnitAttribute : Attribute
    {
        /// <summary>
        /// Gets or sets the unit abbreviation.
        /// </summary>
        public string Unit { get; set; }
        /// <summary>
        /// Initializes a new instance of the <see cref="UnitAttribute"/> class.
        /// </summary>
        /// <param name="unit">The unit.</param>
        public UnitAttribute(string unit)
        {
            Unit = unit;
        }
        /// <summary>
        /// Returns the unit abbreviation for an enum value 
        /// </summary>
        /// <param name="value">The enum value.</param>
        public static string GetEnumUnit(Enum value)
        {
            FieldInfo fi = value.GetType().GetField(value.ToString());
            var attributes = (UnitAttribute[])fi.GetCustomAttributes(typeof(UnitAttribute), false);
            return attributes[0].Unit;
        }
    }
}