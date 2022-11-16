using System.Collections.Generic;

namespace Extensions
{
    public static class ListExtensions
    {
        private static System.Random _random = new System.Random();
        public static T PickRandom<T>(this IList<T> source) => source[_random.Next(source.Count)];

        public static T PopRandom<T>(this IList<T> source)
        {
            var value = source[_random.Next(source.Count)];
            source.Remove(value);
            return value;
        }
    }
}
