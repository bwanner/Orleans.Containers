using System.Collections.Generic;
using System.Threading.Tasks;

namespace Orleans.Streams
{
    /// <summary>
    /// Provides a stream of data.
    /// </summary>
    /// <typeparam name="TOut">Type of data.</typeparam>
    public interface ITransactionalStreamProvider : ITransactionalStreamTearDown
    {
        /// <summary>
        /// Get identities of the provided output streams.
        /// </summary>
        /// <returns></returns>
        Task<IList<StreamIdentity>> GetOutputStreams();
    }
}