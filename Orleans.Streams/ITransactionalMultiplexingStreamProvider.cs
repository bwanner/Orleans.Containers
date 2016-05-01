using System.Threading.Tasks;

namespace Orleans.Streams
{
    public interface ITransactionalMultiplexingStreamProvider<TOut> : ITransactionalStreamProvider<TOut>
    {
        Task SetOutputMultiplex(uint factor = 1);
    }
}