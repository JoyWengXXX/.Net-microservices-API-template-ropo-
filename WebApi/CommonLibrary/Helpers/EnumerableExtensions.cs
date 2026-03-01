
namespace CommonLibrary.Helpers
{
    public static class EnumerableExtensions
    {
        // ÂX®i¤èªk TakeWhileInclusive
        public static IEnumerable<T> TakeWhileInclusive<T>(this IEnumerable<T> source, Func<T, bool> predicate)
        {
            bool conditionMet = false;
            foreach (var item in source)
            {
                yield return item;
                if (!predicate(item))
                {
                    conditionMet = true;
                    break;
                }
            }

            // Include the item that met the condition
            if (!conditionMet)
            {
                foreach (var item in source.SkipWhile(predicate).Take(1))
                {
                    yield return item;
                }
            }
        }
    }   
}

