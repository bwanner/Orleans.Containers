using System;
using System.Threading.Tasks;

namespace Orleans.Streams
{
    /// <summary>
    /// Executes where operations on a stream and forwards results evaluating to 'true' to its output stream.
    /// </summary>
    public interface IStreamProcessorWhereNodeGrain<TIn> : IStreamProcessorNodeGrain<TIn, TIn>
    {
        /// <summary>
        /// Set the where function.
        /// </summary>
        /// <param name="function">Filter function for each item. Evaluation to 'true' will forward results to the output stream.</param>
        /// <returns></returns>
        Task SetFunction(Func<TIn, bool> function);
    }
}