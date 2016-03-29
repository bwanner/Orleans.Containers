using System;
using System.Threading.Tasks;
using Orleans.Streams;

namespace Orleans.Collections
{
    /// <summary>
    /// Grain holding a collection of elements of type T.
    /// </summary>
    /// <typeparam name="T">Element type that is held.</typeparam>
    internal interface IContainerNodeGrain<T> : IGrainWithGuidKey, ICollectionOperations<T>, IElementEnumeratorNode<T>, IBatchItemAdder<T>, IElementExecutor<T>, IStreamProcessorNodeGrain<T, ContainerElement<T>>
    {
    }
}