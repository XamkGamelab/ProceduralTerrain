using System.Collections.Generic;
using System.Linq;
using System;

namespace ExtensionMethods
{
    public static class Extensions
    {
        static Random random;

        static Extensions()
        {
            random = new Random();
        }

        /// <summary>
        /// Randomly shuffle stack with array
        /// </summary>
        /// <typeparam name="T">Generic type T</typeparam>
        /// <param name="stack">This stack</param>
        public static void Shuffle<T>(this Stack<T> stack)
        {
            var values = stack.ToArray();
            stack.Clear();
            foreach (var value in values.OrderBy(x => random.Next()))
                stack.Push(value);
        }
    }
}