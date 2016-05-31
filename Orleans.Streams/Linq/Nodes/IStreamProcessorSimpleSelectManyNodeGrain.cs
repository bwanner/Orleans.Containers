using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Orleans.Streams
{
    /// <summary>
    /// Executes a select many operation on a stream and forwards it to its output stream.
    /// </summary>
    public interface IStreamProcessorSimpleSelectManyNodeGrain<TIn, TOut> : IStreamProcessorNodeGrain<TIn, TOut>
    {
        /// <summary>
        /// Set the select many function.
        /// </summary>
        /// <param name="function">Selection function for each item.</param>
        /// <returns></returns>
        Task SetFunction(SerializableFunc<TIn, IEnumerable<TOut>> function);
    }
}