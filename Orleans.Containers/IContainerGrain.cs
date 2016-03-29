using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Orleans.Streams;

namespace Orleans.Collections
{
    public interface IContainerGrain<T> : IGrainWithGuidKey, ICollectionOperations<T>, IElementEnumerator<T>, IBatchWriteable<T>, IElementExecutor<T>, IStreamProcessorAggregate<T, ContainerElement<T>>
    {
        /// <summary>
        /// Sets the number of containers to use for this collection. Only increase of size is supported.
        /// </summary>
        /// <param name="numContainer">New number of containers.</param>
        /// <returns></returns>
        Task SetNumberOfNodes(int numContainer);
    }
}