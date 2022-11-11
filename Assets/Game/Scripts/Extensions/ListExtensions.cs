using System.Collections.Generic;
using UnityEngine;

namespace Extensions
{
    public static class ListExtensions
    {
        private static System.Random _random = new System.Random();
        public static Vector2Int PickRandom(this IList<Vector2Int> source) => source[_random.Next(source.Count)];
    }
}
