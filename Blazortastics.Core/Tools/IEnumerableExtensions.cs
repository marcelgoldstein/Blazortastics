using System;
using System.Collections.Generic;

namespace Blazortastics.Core.Tools
{
    public static class IEnumerableExtensions
    {
        /// <summary>
        /// Flattens an object hierarchy.
        /// </summary>
        /// <param name="rootLevel">The root level in the hierarchy.</param>
        /// <param name="nextLevel">A function that returns the next level below a given item.</param>
        /// <returns><![CDATA[An IEnumerable<T> containing every item from every level in the hierarchy.]]></returns>
        public static IEnumerable<T> Flatten<T>(this IEnumerable<T> rootLevel, Func<T, IEnumerable<T>> nextLevel)
        {
            List<T> accumulation = new List<T>();
            flattenLevel<T>(accumulation, rootLevel, nextLevel);
            return accumulation;
        }

        /// <summary>
        /// Recursive helper method that traverses a hierarchy, accumulating items along the way.
        /// </summary>
        /// <param name="accumulation">A collection in which to accumulate items.</param>
        /// <param name="currentLevel">The current level we are traversing.</param>
        /// <param name="nextLevel">A function that returns the next level below a given item.</param>
        private static void flattenLevel<T>(List<T> accumulation, IEnumerable<T> currentLevel, Func<T, IEnumerable<T>> nextLevel)
        {
            foreach (T item in currentLevel)
            {
                accumulation.Add(item);
                flattenLevel<T>(accumulation, nextLevel(item), nextLevel);
            }
        }
    }
}
