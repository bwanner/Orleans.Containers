using System;
using System.Threading.Tasks;

namespace Orleans.Streams.Linq.Aggregates
{
    public interface IStreamProcessorWhereAggregate<TIn> : IStreamProcessorAggregate<TIn, TIn>
    {
        Task SetFunction(Func<TIn, bool> function);
    }
}