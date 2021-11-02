using System;
using System.Collections;
using System.Collections.Generic;

namespace Synergy.Common.Domain.Models.Extensions
{
    public static class DataDumpExtensions
    {
        public static IDictionary<string, object> ToDataDump<T>(this T source, string key = null, int deep = 0)
        {
            var result = new Dictionary<string, object>();

            if (deep == 5)
            {
                return result;
            }

            if (source == null)
            {
                result.Add(key ?? string.Empty, string.Empty);
                return result;
            }

            if (source is IConvertible val)
            {
                result.Add(key ?? string.Empty, val);
                return result;
            }

            if (source is IEnumerable items)
            {
                var count = 1;
                foreach (var item in items)
                {
                    foreach (var x in ToDataDump(item, $"{key}[{count}]", deep + 1))
                    {
                        result.Add(x.Key, x.Value);
                    }

                    count++;
                }

                return result;
            }

            var properties = source.GetType().GetProperties();
            foreach (var property in properties)
            {
                var subKey = string.IsNullOrWhiteSpace(key) ? $"{property.Name}" : $"{key}.{property.Name}";
                foreach (var x in ToDataDump(property.GetValue(source, null), subKey, deep + 1))
                {
                    result.Add(x.Key, x.Value);
                }
            }

            return result;
        }
    }
}
