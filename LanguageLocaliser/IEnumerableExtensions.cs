namespace LanguageLocaliser
{
    internal static class IEnumerableExtenions
    {
        /// <summary>
        /// Returns the first index of an item that matches criteria
        /// </summary>
        /// <typeparam name="T">The type of the element</typeparam>
        /// <param name="items">The items to get the index for</param>
        /// <param name="matchedOn">The criteria for an item to match</param>
        /// <returns>The numerical index of the item or -1 if not found.</returns>
        public static int FirstIndexOf<T>(this IEnumerable<T> items, Func<int, T, bool> matchedOn)
        {
            return
                items.Select((value, index) => new { value, index })
                .Where(pair => matchedOn(pair.index, pair.value))
                .Select(pair => (int?)pair.index)
                .FirstOrDefault() ?? -1;
        }

        /// <summary>
        /// Counts the matching contiguous elements for two enumerables
        /// </summary>
        /// <typeparam name="T">The type of the value to enumerate</typeparam>
        /// <param name="e1">The first enumerable</param>
        /// <param name="e2">The second enumerable</param>
        /// <param name="start">The index element</param>
        /// <returns>The number of items that matched</returns>
        public static int CountMatching<T>(this IEnumerable<T> e1, IEnumerable<T> e2, int start = 0) where T : IEquatable<T>
        {
            return
                e1
                // Simultaneous Iteration
                .Zip(e2, (i1, i2) => new { i1, i2 })
                // Skip to starting element
                .Skip(start)
                // Get Value and Index
                .Select((v, idx) => new { v, idx })
                // Skip all Matching
                .SkipWhile(v => v.v.i1.Equals(v.v.i2))
                // Take next item if available
                .Take(1)
                // Box the last value if available
                .Select(v => (int?)v.idx)
                // Get next or null if not available
                .FirstOrDefault()
                // Get max length-start if no items left
                ?? Math.Min(Math.Max(e1.Count() - start, 0), Math.Max(e2.Count() - start, 0));
        }
    }
}