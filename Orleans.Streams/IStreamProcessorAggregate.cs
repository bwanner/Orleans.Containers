using System.Collections.Generic;
using System.Threading.Tasks;

namespace Orleans.Streams
{
    /// <summary>
    ///     Transforms data from TIn to TOut using multiple IStreamProcessorNode.
    /// </summary>
    /// <typeparam name="TIn">Data input type.</typeparam>
    /// <typeparam name="TOut">Data output type.</typeparam>
    public interface IStreamProcessorAggregate<TIn, TOut> : IGrainWithGuidKey, ITransactionalStreamConsumerAggregate,
        ITransactionalStreamProvider<TOut>
    {
        Task<IList<SiloLocationStreamIdentity>> GetOutputStreamsWithSourceLocation();
    }
}