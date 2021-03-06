﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Orleans.Streams;
using Orleans.Streams.Linq;

namespace Orleans.Collections.Utilities
{
    public static class CollectionExtensions
    {
        /// <summary>
        /// Add elements to a collection in batches and parallel between containers in the collection.
        /// </summary>
        /// <typeparam name="T">Type of items to add.</typeparam>
        /// <typeparam name="TX"></typeparam>
        /// <param name="consumers">Consumers to send data to.</param>
        /// <param name="elements">Items to be added.</param>
        /// <returns>Task that is completed after all items are added.</returns>
        public static async Task<Dictionary<ContainerElementReference<T>, T>> BatchAddReturnDictionary<TX, T>(this ICollection<TX> consumers, IReadOnlyCollection<T> elements, int batchSize = 4096) where TX : IBatchItemAdder<T>
        {
            var elementReferences = await consumers.BatchAdd(elements);

            return elementReferences.Zip(elements, (reference, value) => new {reference, value}).ToDictionary(x => x.reference, x => x.value);
        }

        /// <summary>
        /// Add elements to a collection in batches and parallel between containers in the collection.
        /// </summary>
        /// <typeparam name="T">Type of items to add.</typeparam>
        /// <param name="collection">Collection to store items to.</param>
        /// <param name="elements">Items to be added.</param>
        /// <returns>Task that is completed after all items are added.</returns>
        public static async Task<Dictionary<ContainerElementReference<T>, T>> BatchAddReturnDictionary<T>(this IContainerGrain<T> collection, IReadOnlyCollection<T> elements)
        {
            var receivers = await collection.GetItemAdders();

            return await BatchAddReturnDictionary(receivers, elements);
        }


        /// <summary>
        /// Add elements to a collection in batches and parallel between containers in the collection.
        /// </summary>
        /// <typeparam name="T">Type of items to add.</typeparam>
        /// <param name="collection">Collection to store items to.</param>
        /// <param name="elements">Items to be added.</param>
        /// <returns>Task that is completed after all items are added.</returns>
        public static async Task<List<ContainerElementReference<T>>> BatchAdd<T>(this IContainerGrain<T> collection, IReadOnlyCollection<T> elements)
        {
            var receivers = await collection.GetItemAdders();

            return await BatchAdd(receivers, elements);
        }

        /// <summary>
        /// Add elements to a collection in batches and parallel between containers in the collection.
        /// </summary>
        /// <typeparam name="T">Type of items to add.</typeparam>
        /// <typeparam name="TX"></typeparam>
        /// <param name="consumers">Consumers to send data to.</param>
        /// <param name="elements">Items to be added.</param>
        /// <returns>Task that is completed after all items are added.</returns>
        public static async Task<List<ContainerElementReference<T>>> BatchAdd<TX,T>(this ICollection<TX> consumers, IReadOnlyCollection<T> elements, int batchSize = 4096) where TX : IBatchItemAdder<T>
        {
            List<IBatchItemAdder<T>> availableReceivers = new List<IBatchItemAdder<T>>((IEnumerable<IBatchItemAdder<T>>) consumers);
            var currentWriteTasks = new Dictionary<Task<IReadOnlyCollection<ContainerElementReference<T>>>, Tuple<IBatchItemAdder<T>, int>>();

            var chunks = elements.Chunks(batchSize);
            var subsetsWithIndex = chunks.Zip(Enumerable.Range(0, chunks.Count), (list, i) => new Tuple<int,List<T>>(i * batchSize, list) );
            List<ContainerElementReference<T>> elementReferences = Enumerable.Repeat<ContainerElementReference<T>>(null, elements.Count).ToList();

            foreach(var subset in subsetsWithIndex) {
                if (availableReceivers.Count == 0)
                {
                    var finishedTask = await Task.WhenAny(currentWriteTasks.Keys);
                    var finishedTuple = currentWriteTasks[finishedTask];
                    var taskResult = await finishedTask;
                    int i = 0;
                    foreach (var result in taskResult)
                    {
                        elementReferences[finishedTuple.Item2 + i] = result;
                        i++;
                    }
                    availableReceivers.Add(finishedTuple.Item1);
                    currentWriteTasks.Remove(finishedTask);
                }

                var chosenReader = availableReceivers.First();
                currentWriteTasks.Add(chosenReader.AddRange(subset.Item2), new Tuple<IBatchItemAdder<T>, int>(chosenReader, subset.Item1));
                availableReceivers.Remove(chosenReader);
            }

            while (currentWriteTasks.Count > 0)
            {
                var finishedTask = await Task.WhenAny(currentWriteTasks.Keys);
                var finishedTuple = currentWriteTasks[finishedTask];
                var taskResult = await finishedTask;
                int i = 0;
                foreach (var result in taskResult)
                {
                    elementReferences[finishedTuple.Item2 + i] = result;
                    i++;
                };
                availableReceivers.Add(finishedTuple.Item1);
                currentWriteTasks.Remove(finishedTask);
            }

            return elementReferences;
        }


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
