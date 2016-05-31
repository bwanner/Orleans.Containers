using System.Collections.Generic;
using System.Threading.Tasks;

namespace Orleans.Streams.Linq.Aggregates
{
    /// <summary>
    ///     Supports defining a selectMany function that is executed by using multiple IStreamProcessorSimpleSelectManyNodeGrain.
    /// </summary>
    /// <typeparam name="TIn">Data input type.</typeparam>
    /// <typeparam name="TOut">Data output type.</typeparam>
    public interface IStreamProcessorSimpleSelectManyAggregate<TIn, TOut> : IStreamProcessorAggregate<TIn, TOut>
    {
        /// <summary>
        ///     Sets the selection function.
        /// </summary>
        /// <param name="function"></param>
        /// <returns></returns>
        Task SetFunction(SerializableFunc<TIn, IEnumerable<TOut>> function);
    }
}