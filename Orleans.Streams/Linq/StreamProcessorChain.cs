using System.Collections.Generic;
using System.Threading.Tasks;

namespace Orleans.Streams.Linq
{
    public class StreamProcessorChainStart<TIn, TOut, TFactory> : StreamProcessorChain<TIn, TOut, TFactory> where TFactory : IStreamProcessorAggregateFactory
    {
        private readonly ITransactionalStreamProvider<TIn> _source;

        public StreamProcessorChainStart(IStreamProcessorAggregate<TIn, TOut> aggregate, ITransactionalStreamProvider<TIn> source, TFactory factory) : base(aggregate, factory)
        {
            _source = source;
        }
    }

    /// <summary>
    /// Represents a chain of stream processors.
    /// </summary>
    /// <typeparam name="TIn"></typeparam>
    /// <typeparam name="TOut"></typeparam>
    /// <typeparam name="TFactory"></typeparam>
    public class StreamProcessorChain<TIn, TOut, TFactory> : IStreamProcessorChain<TOut, TFactory> where TFactory : IStreamProcessorAggregateFactory
    {
        private readonly IStreamProcessorChain<TIn, TFactory> _previous;
        private bool _tearDownExecuted = false;
        public IStreamProcessorAggregate<TIn, TOut> Aggregate { get; private set; }

        public TFactory Factory { get; private set; }


        public StreamProcessorChain(IStreamProcessorAggregate<TIn, TOut> aggregate, TFactory factory)
        {
            Aggregate = aggregate;
            Factory = factory;
            _previous = null;
        }

        public StreamProcessorChain(IStreamProcessorAggregate<TIn, TOut> aggregate, IStreamProcessorChain<TIn, TFactory> previous)
        {
            Aggregate = aggregate;
            Factory = previous.Factory;
            _previous = previous;
        }

        public async Task<IList<StreamIdentity>> GetOutputStreams()
        {
            return await Aggregate.GetOutputStreams();
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

    public interface IStreamProcessorChain<TOut, TFactory> : ITransactionalStreamProvider<TOut> where TFactory : IStreamProcessorAggregateFactory
    {
        TFactory Factory { get; }
    }
}