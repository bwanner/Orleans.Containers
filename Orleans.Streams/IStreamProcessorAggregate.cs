namespace Orleans.Streams
{
    /// <summary>
    /// Transforms data from TIn to TOut using processing nodes of type TNode.
    /// </summary>
    /// <typeparam name="TIn">Data input type.</typeparam>
    /// <typeparam name="TOut">Data output type.</typeparam>>
    /// <typeparam name="TNode">Processing node type.</typeparam>
    public interface IStreamProcessorAggregate<TIn, TOut, TNode> : IStreamProcessorAggregate<TIn, TOut> where TNode : IStreamProcessorNodeGrain<TIn, TOut>
    {
    }

    /// <summary>
    ///     Transforms data from TIn to TOut.
    /// </summary>
    /// <typeparam name="TIn">Data input type.</typeparam>
    /// <typeparam name="TOut">Data output type.</typeparam>
    public interface IStreamProcessorAggregate<TIn, TOut> : IGrainWithGuidKey, ITransactionalStreamConsumerAggregate,
        ITransactionalStreamProviderAggregate<TOut>
    {
    }
}