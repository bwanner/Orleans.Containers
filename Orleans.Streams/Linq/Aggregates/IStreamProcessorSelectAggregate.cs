using System.Threading.Tasks;

namespace Orleans.Streams.Linq.Aggregates
{
    /// <summary>
    ///     Supports defining a select function that is executed by using multiple IStreamProcessorSelectNodeGrain.
    /// </summary>
    /// <typeparam name="TIn">Data input type.</typeparam>
    /// <typeparam name="TOut">Data output type.</typeparam>
    public interface IStreamProcessorSelectAggregate<TIn, TOut> : IStreamProcessorAggregate<TIn, TOut>
    {
        /// <summary>
        ///     Sets the selection function.
        /// </summary>
        /// <param name="function"></param>
        /// <returns></returns>
        Task SetFunction(SerializableFunc<TIn, TOut> function);
    }
}