namespace Orleans.Streams
{
    /// <summary>
    /// Executes operations on a stream and forwards it to its output stream.
    /// </summary>
    public interface IStreamProcessorNodeGrain<TIn, TOut> : IGrainWithGuidKey, ITransactionalStreamProvider, ITransactionalStreamConsumer
    {
        // TODO parallel function
    }
}