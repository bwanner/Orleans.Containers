using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Orleans.Streams
{
    public static class StreamHelper
    {
        /// <summary>
        /// Chunk a collection of elements into parts of a defined size.
        /// TODO maybe replace with Utils.BatchIEnumerable() from Orleans.
        /// </summary>
        /// <typeparam name="T">Type of elements in the collection.</typeparam>
        /// <param name="elements">The collection.</param>
        /// <param name="chunkSize">Size of the chunks.</param>
        /// <returns>A collection consisting of multiple collections with size less or equal chunkSize.</returns>
        public static List<List<T>> Chunks<T>(this IReadOnlyCollection<T> elements, int chunkSize)
        {
            List<List<T>> chunks = new List<List<T>>();
            List<T> curList = new List<T>();

            int i = 0;
            foreach (var element in elements)
            {

                if (curList.Count == chunkSize)
                {
                    chunks.Add(curList);
                    curList = new List<T>();
                }

                curList.Add(element);
                i++;
            }

            if (curList.Count > 0)
            {
                chunks.Add(curList);
            }

            return chunks;
        }

        public static IEnumerable<T> Repeat<T>(this IEnumerable<T> source)
        {
            while (true)
            {
                foreach (var item in source)
                {
                    yield return item;
                }
            }
        }

        /// <summary>
        /// Wraps a single item into a List.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static List<T> SingleValueToList<T>(this T obj)
        {
            return new List<T> {obj};
        } 
    }
}