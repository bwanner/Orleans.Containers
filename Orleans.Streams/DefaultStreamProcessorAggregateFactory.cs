using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Orleans.Streams.Linq.Aggregates;

namespace Orleans.Streams
{
    /// <summary>
    /// Default aggregate factory for stream processing using stateless processors aggregates.
    /// </summary>
    public class DefaultStreamProcessorAggregateFactory : IStreamProcessorAggregateFactory
    {
        private readonly IGrainFactory _grainFactory;

        public DefaultStreamProcessorAggregateFactory(IGrainFactory grainFactory)
        {
            _grainFactory = grainFactory;
        }

        public IGrainFactory GrainFactory
        {
            get { return _grainFactory; }
        }

        public async Task<IStreamProcessorAggregate<TIn, TOut>> CreateSelect<TIn, TOut>(Expression<Func<TIn, TOut>> selectionFunc, IList<StreamIdentity> streamIdentities)
        {
            var processorAggregate =_grainFactory.GetGrain<IStreamProcessorSelectAggregate<TIn, TOut>>(Guid.NewGuid());
            
            await processorAggregate.SetFunction(selectionFunc);
            await processorAggregate.SetInput(streamIdentities);

            return processorAggregate;
        }

        public async Task<IStreamProcessorAggregate<TIn, TIn>> CreateWhere<TIn>(Expression<Func<TIn, bool>> filterFunc, IList<StreamIdentity> streamIdentities)
        {
            var processorAggregate = _grainFactory.GetGrain<IStreamProcessorWhereAggregate<TIn>>(Guid.NewGuid());

            await processorAggregate.SetFunction(filterFunc);
            await processorAggregate.SetInput(streamIdentities);

            return processorAggregate;
        }

        public Task<IStreamProcessorAggregate<TIn, TOut>> CreateSimpleSelectMany<TIn, TOut>(Expression<Func<TIn, IEnumerable<TOut>>> selectionFunc, IList<StreamIdentity> list)
        {
            throw new NotImplementedException();
        }

        public Task<IStreamProcessorAggregate<TIn, TOut>> CreateSelectMany<TIn, TIntermediate, TOut>(Expression<Func<TIn, IEnumerable<TIntermediate>>> collectionSelectorFunc, Expression<Func<TIn, TIntermediate, TOut>> resultSelectorFunc, IList<StreamIdentity> streamIdentities)
        {
            throw new NotImplementedException();
        }
    }
}