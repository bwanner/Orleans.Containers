using System.Collections.Generic;
using System.Threading.Tasks;

namespace Orleans.Streams.Linq
{
    public class StreamProcessorChainStart<TIn, TOut> : StreamProcessorChain<TIn, TOut>
    {
        private readonly ITransactionalStreamProviderAggregate<TIn> _source;

        public StreamProcessorChainStart(IStreamProcessorAggregate<TIn, TOut> aggregate, ITransactionalStreamProviderAggregate<TIn> source, IStreamProcessorAggregateFactory factory) : base(aggregate, factory)
        {
            _source = source;
        }
    }

    /// <summary>
    /// Represents a chain of stream processors.
    /// </summary>
    /// <typeparam name="TIn"></typeparam>
    /// <typeparam name="TOut"></typeparam>
    public class StreamProcessorChain<TIn, TOut> : IStreamProcessorChain<TOut>
    {
        private readonly IStreamProcessorChain<TIn> _previous;
        private bool _tearDownExecuted = false;
        internal IStreamProcessorAggregate<TIn, TOut> Aggregate { get; private set; }

        public IStreamProcessorAggregateFactory Factory { get; private set; }


        public StreamProcessorChain(IStreamProcessorAggregate<TIn, TOut> aggregate, IStreamProcessorAggregateFactory factory)
        {
            Aggregate = aggregate;
            Factory = factory;
            _previous = null;
        }

        public StreamProcessorChain(IStreamProcessorAggregate<TIn, TOut> aggregate, IStreamProcessorChain<TIn> previous)
        {
            Aggregate = aggregate;
            Factory = previous.Factory;
            _previous = previous;
        }

        public async Task<IList<StreamIdentity>> GetStreamIdentities()
        {
            return await Aggregate.GetStreamIdentities();
        }

        public async Task TearDown()
        {
            _tearDownExecuted = true;
            if (_previous != null)
            {
                await _previous.TearDown();
            }
            else
            {
                await Aggregate.TearDown();
            }
        }

        public Task<bool> IsTearedDown()
        {
            return Task.FromResult(_tearDownExecuted);
        }
    }

    public interface IStreamProcessorChain<TOut> : ITransactionalStreamProviderAggregate<TOut>
    {
        IStreamProcessorAggregateFactory Factory { get; }
    }
}