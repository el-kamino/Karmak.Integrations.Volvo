using System;
using System.Collections.Generic;
using System.Linq;

namespace Karmak.Integrations.Volvo.React.Utils
{
    public static class EnumerableExtensions
    {
        public static bool NotNullAndContains<T>(this IEnumerable<T> collection, T item)
        {
            return collection != null && collection.Contains(item);
        }

        public static bool NotNullAndAny<T>(this IEnumerable<T> collection, Predicate<T> predicate)
        {
            return collection != null && collection.Any(item => predicate(item));
        }

        public static bool NotNullAndAny<T>(this IEnumerable<T> collection)
        {
            return collection != null && collection.Any();
        }

        public static (IEnumerable<T>, IEnumerable<T>) SplitEvenly<T>(this IEnumerable<T> enumerable) {
            var collection = enumerable.ToArray();
            var length = collection.Length;
            var leftLength = length / 2 + length % 2;
            var rightLength = length / 2;
            var left = new T[leftLength];
            for (var i = 0; i < leftLength; i++) {
                left[i] = collection[i];
            }
            var right = new T[rightLength];
            for (var i = 0; i < rightLength; i++) {
                right[i] = collection[i + leftLength];
            }
            return (left, right);
        }

        public static IEnumerable<T> RemoveNulls<T>(this IEnumerable<T> collection)
        {
            return collection.Where(item => item != null);
        }
    }
}