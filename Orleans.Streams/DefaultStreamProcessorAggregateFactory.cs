using System;
using System.Collections.Generic;
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

        public async Task<IStreamProcessorSelectAggregate<TIn, TOut>> CreateSelect<TIn, TOut>(Func<TIn, TOut> selectionFunc, IList<TransactionalStreamIdentity<TIn>> streamIdentities)
        {
            var processorAggregate =_factory.GetGrain<IStreamProcessorSelectAggregate<TIn, TOut>>(Guid.NewGuid());

            await processorAggregate.SetFunction(selectionFunc);
            await processorAggregate.SetInput(streamIdentities);

            return processorAggregate;
        }

        public async Task<IStreamProcessorWhereAggregate<TIn>> CreateWhere<TIn>(Func<TIn, bool> filterFunc, IList<TransactionalStreamIdentity<TIn>> streamIdentities)
        {
            var processorAggregate = _factory.GetGrain<IStreamProcessorWhereAggregate<TIn>>(Guid.NewGuid());

            await processorAggregate.SetFunction(filterFunc);
            await processorAggregate.SetInput(streamIdentities);

            return processorAggregate;
        }
    }
}