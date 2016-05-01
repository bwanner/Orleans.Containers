using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Orleans.Streams.Linq.Aggregates;

namespace Orleans.Streams
{
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
    }
}