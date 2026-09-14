using System;
using System.Collections.Generic;
using System.Linq;

namespace Karmak.Integrations.Volvo.Warranty.Contracts
{
    public static class EqualityExtensions
    {

        public static bool Equals<T>(T item1, T item2) where T : class, IEquatable<T>
        {
            if (item1 == null && item2 == null)
            {
                return true;
            }

            if (item1 == null || item2 == null)
            {
                return false;
            }

            return item1.Equals(item2);
        }

        public static bool Equals<T>(IEnumerable<T> collection1, IEnumerable<T> collection2) where T : class, IEquatable<T>
        {
            if (collection1 == null && collection2 == null)
            {
                return true;
            }

            if (collection1 == null || collection2 == null)
            {
                return false;
            }


            if (collection1.Count() != collection2.Count())
            {
                return false;
            }

            var trimmedCollection1 = collection1.Where(x => x != null);
            var trimmedCollection2 = collection2.Where(x => x != null);

            foreach (var item in trimmedCollection1)
            {
                if (!trimmedCollection2.Any(x => x.Equals(item)))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
