using System.Collections.Generic;

namespace Karmak.Integrations.Volvo.React.Utils
{
    public static class IListExtensions
    {
        public static bool IsEmpty<T>(this IList<T> list)
        {
            return list.Count == 0;
        }
    }
}