using System;
using System.Collections.Generic;
using System.Text;

namespace VisualUninstaller
{
    static class Utils
    {
        public static IEnumerable<T> Filter<T>(IEnumerable<T> enumerable, Predicate<T> predicate)
        {
            if (enumerable == null) throw new ArgumentNullException("enumerable");
            if (predicate == null) throw new ArgumentNullException("predicate");

            foreach (T elem in enumerable)
            {
                if (predicate(elem)) yield return elem;
            }
        }
    }
}
