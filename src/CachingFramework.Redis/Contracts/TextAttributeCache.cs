using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CachingFramework.Redis.Contracts
{
    public class TextAttributeCache<T>
    {
        private readonly Dictionary<T, string> ValueTextMapping = new Dictionary<T, string>();
        private readonly Dictionary<string, T> TextValueMapping = new Dictionary<string, T>();

        private static readonly Lazy<TextAttributeCache<T>> _lazy = new Lazy<TextAttributeCache<T>>(() => new TextAttributeCache<T>());
        public static TextAttributeCache<T> Instance
        {
            get { return _lazy.Value; }
        }

        internal TextAttributeCache()
        {
            BuildEnumMapping();
        }

        public string GetEnumText(T value)
        {
            string text;
            return ValueTextMapping.TryGetValue(value, out text) ? text : string.Empty;
        }

        public T GetEnumValue(string text)
        {
            T value;
            return TextValueMapping.TryGetValue(text, out value) ? value : default(T);
        }

        private void BuildEnumMapping()
        {

            var fields = typeof(T).GetFields(BindingFlags.Public | BindingFlags.Static);
            foreach (var field in fields)
            {
                T value = (T)field.GetRawConstantValue();
                var attr = (TextAttribute[])field.GetCustomAttributes(typeof(TextAttribute), false);
                if (attr.Length > 0)
                {
                    var text = attr[0].Text;
                    ValueTextMapping.Add(value, text);
                    TextValueMapping.Add(text, value);
                }
            }
        }
    }
}
