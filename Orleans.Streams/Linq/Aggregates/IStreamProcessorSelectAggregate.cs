using System;
using System.Threading.Tasks;

namespace Orleans.Streams.Linq.Aggregates
{
    public interface IStreamProcessorSelectAggregate<TIn, TOut> : IStreamProcessorAggregate<TIn, TOut>
    {
        Task SetFunction(SerializableFunc<TIn, TOut> function);
    }

    
}