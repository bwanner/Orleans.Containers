using System.Collections.Generic;
using System.Threading.Tasks;

namespace Orleans.Streams
{
    /// <summary>
    /// Provides multiple streams of data.
    /// </summary>
    /// <typeparam name="TOut">Type of data.</typeparam>
    public interface ITransactionalStreamProviderAggregate<TOut> : ITransactionalStreamProvider<TOut>, ITransactionalStreamTearDown
    {
    }
}