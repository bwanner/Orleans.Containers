using System.Threading.Tasks;

namespace Orleans.Streams.Linq.Aggregates
{
    /// <summary>
    ///     Supports defining a where function that is executed by using multiple IStreamProcessorWhereNodeGrain.
    /// </summary>
    /// <typeparam name="TIn">Data input/output type.</typeparam>
    public interface IStreamProcessorWhereAggregate<TIn> : IStreamProcessorAggregate<TIn, TIn>
    {
        /// <summary>
        ///     Define the where function.
        /// </summary>
        /// <param name="function"></param>
        /// <returns></returns>
        Task SetFunction(SerializableFunc<TIn, bool> function);
    }
}