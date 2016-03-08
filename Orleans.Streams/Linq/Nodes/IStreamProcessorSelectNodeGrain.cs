using System;
using System.Threading.Tasks;

namespace Orleans.Streams
{
    /// <summary>
    /// Executes select operation on a stream and forwards it to its output stream.
    /// </summary>
    public interface IStreamProcessorSelectNodeGrain<TIn, TOut> : IStreamProcessorNodeGrain<TIn, TOut>
    {
        /// <summary>
        /// Set the select function.
        /// </summary>
        /// <param name="function">Selection function for each item.</param>
        /// <returns></returns>
        Task SetFunction(Func<TIn, TOut> function);
    }
}