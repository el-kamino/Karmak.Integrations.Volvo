using System.Collections.Generic;
using System.Linq;

namespace Karmak.Integrations.Volvo.React.Utils
{
    public static class DictionaryExtensions {
        public static Dictionary<U, V> Merge<U, V>(this IDictionary<U, V> original, Dictionary<U, V> other) {
            var result = new Dictionary<U, V>();
            var allKeyValues = original
                .Concat(other ?? new Dictionary<U, V>());
            foreach (var (key, value) in allKeyValues) {
                result[key] = value;
            }
            return result;
        }
    }
}