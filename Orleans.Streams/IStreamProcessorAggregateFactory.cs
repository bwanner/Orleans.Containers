using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Orleans.Streams.Linq.Aggregates;

namespace Orleans.Streams
{
    /// <summary>
    /// Creates stream processing aggregates.
    /// </summary>
    public interface IStreamProcessorAggregateFactory
    {
        IGrainFactory GrainFactory { get; }

        Task<IStreamProcessorAggregate<TIn, TOut>> CreateSelect<TIn, TOut>(Expression<Func<TIn, TOut>> selectionFunc, IList<StreamIdentity> streamIdentities);
        
        Task<IStreamProcessorAggregate<TIn, TIn>> CreateWhere<TIn>(Expression<Func<TIn, bool>> filterFunc, IList<StreamIdentity> streamIdentities);
    }
}