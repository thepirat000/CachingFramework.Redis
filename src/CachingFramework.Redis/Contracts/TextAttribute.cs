using System;
using System.Reflection;

namespace CachingFramework.Redis.Contracts
{
    /// <summary>
    /// Attribute to indicate the Text abbreviation in redis
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, Inherited = false, AllowMultiple = true)]
    internal sealed class TextAttribute : Attribute
    {
        /// <summary>
        /// Gets or sets the Text abbreviation.
        /// </summary>
        public string Text { get; set; }
        /// <summary>
        /// Initializes a new instance of the <see cref="TextAttribute"/> class.
        /// </summary>
        /// <param name="text">The Text.</param>
        public TextAttribute(string text)
        {
            Text = text;
        }
        /// <summary>
        /// Returns the Text abbreviation for an enum value 
        /// </summary>
        /// <param name="value">The enum value.</param>
        public static string GetEnumText(Enum value)
        {
            FieldInfo fi = value.GetType().GetField(value.ToString());
            var attributes = (TextAttribute[])fi.GetCustomAttributes(typeof(TextAttribute), false);
            return attributes[0].Text;
        }
    }
}