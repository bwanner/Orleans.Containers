using System.Collections.Generic;

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
    }
}