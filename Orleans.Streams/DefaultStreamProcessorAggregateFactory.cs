using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Orleans.Streams.Linq.Aggregates;

namespace Orleans.Streams
{
    public class DefaultStreamProcessorAggregateFactory : IStreamProcessorAggregateFactory
    {
        private readonly IGrainFactory _factory;

        public DefaultStreamProcessorAggregateFactory(IGrainFactory factory)
        {
            _factory = factory;
        }

        public IStreamProcessorSelectAggregate<TIn, TOut> CreateSelect<TIn, TOut>()
        {
            return _factory.GetGrain<IStreamProcessorSelectAggregate<TIn, TOut>>(Guid.NewGuid());
        }

        public IGrainFactory Factory
        {
            get { return _factory; }
        }

        public async Task<IStreamProcessorSelectAggregate<TIn, TOut>> CreateSelect<TIn, TOut>(Expression<Func<TIn, TOut>> selectionFunc, IList<StreamIdentity> streamIdentities)
        {
            var processorAggregate =_factory.GetGrain<IStreamProcessorSelectAggregate<TIn, TOut>>(Guid.NewGuid());
            
            await processorAggregate.SetFunction(new SerializableFunc<TIn, TOut>(selectionFunc));
            await processorAggregate.SetInput(streamIdentities);

            return processorAggregate;
        }

        public async Task<IStreamProcessorWhereAggregate<TIn>> CreateWhere<TIn>(Expression<Func<TIn, bool>> filterFunc, IList<StreamIdentity> streamIdentities)
        {
            var processorAggregate = _factory.GetGrain<IStreamProcessorWhereAggregate<TIn>>(Guid.NewGuid());

            await processorAggregate.SetFunction(new SerializableFunc<TIn, bool>(filterFunc));
            await processorAggregate.SetInput(streamIdentities);

            return processorAggregate;
        }
    }
}