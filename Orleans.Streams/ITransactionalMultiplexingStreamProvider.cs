using System.Threading.Tasks;

namespace Orleans.Streams
{
    /// <summary>
    /// Provides a certain number of streams streaming data of type TOut.
    /// </summary>
    /// <typeparam name="TOut">Data type to stream.</typeparam>
    public interface ITransactionalMultiplexingStreamProvider<TOut> : ITransactionalStreamProvider<TOut>
    {
        /// <summary>
        /// Set multiplier for streams to use.
        /// </summary>
        /// <param name="factor">Factor to multiply incoming streams with.</param>
        /// <returns></returns>
        Task SetOutputMultiplex(uint factor = 1);
    }
}