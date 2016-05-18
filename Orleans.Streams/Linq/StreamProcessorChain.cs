using System.Collections.Generic;
using System.Threading.Tasks;

namespace Orleans.Streams.Linq
{
    /// <summary>
    ///     Represents the start of a chain of stream processors.
    /// </summary>
    /// <typeparam name="TIn"></typeparam>
    /// <typeparam name="TOut"></typeparam>
    /// <typeparam name="TFactory"></typeparam>
    public class StreamProcessorChainStart<TIn, TOut, TFactory> : StreamProcessorChain<TIn, TOut, TFactory>
        where TFactory : IStreamProcessorAggregateFactory
    {
        private readonly ITransactionalStreamProvider<TIn> _source;

        public StreamProcessorChainStart(IStreamProcessorAggregate<TIn, TOut> aggregate, ITransactionalStreamProvider<TIn> source, TFactory factory)
            : base(aggregate, factory)
        {
            _source = source;
        }
    }

    /// <summary>
    ///     Represents a chain of stream processors.
    /// </summary>
    /// <typeparam name="TIn"></typeparam>
    /// <typeparam name="TOut"></typeparam>
    /// <typeparam name="TFactory"></typeparam>
    public class StreamProcessorChain<TIn, TOut, TFactory> : IStreamProcessorChain<TOut, TFactory> where TFactory : IStreamProcessorAggregateFactory
    {
        private readonly IStreamProcessorChain<TIn, TFactory> _previous;
        private bool _tearDownExecuted;

        /// <summary>
        /// Aggregate responsible for this step.
        /// </summary>
        public IStreamProcessorAggregate<TIn, TOut> Aggregate { get; }

        public StreamProcessorChain(IStreamProcessorAggregate<TIn, TOut> aggregate, TFactory factory)
        {
            Aggregate = aggregate;
            Factory = factory;
            _previous = null;
        }

        internal StreamProcessorChain(IStreamProcessorAggregate<TIn, TOut> aggregate, IStreamProcessorChain<TIn, TFactory> previous)
        {
            Aggregate = aggregate;
            Factory = previous.Factory;
            _previous = previous;
        }

        /// <summary>
        /// Factory to create new aggregates for stream processing.
        /// </summary>
        public TFactory Factory { get; }

        /// <summary>
        /// Get output streams of this stream processing step.
        /// </summary>
        /// <returns></returns>
        public async Task<IList<StreamIdentity>> GetOutputStreams()
        {
            return await Aggregate.GetOutputStreams();
        }

        /// <summary>
        /// Checks if this entity is teared down.
        /// </summary>
        /// <returns></returns>
        public Task<bool> IsTearedDown()
        {
            return Task.FromResult(_tearDownExecuted);
        }

        /// <summary>
        /// Unsubscribes from all streams this entity consumes and remove references to stream this entity produces.
        /// </summary>
        /// <returns></returns>
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
    }

    /// <summary>
    /// Interface describing a the output and factory of a step in a stream processor chain.
    /// </summary>
    /// <typeparam name="TOut">Data output type.</typeparam>
    /// <typeparam name="TFactory">Factory to create new processor aggregates.</typeparam>
    public interface IStreamProcessorChain<TOut, TFactory> : ITransactionalStreamProvider<TOut> where TFactory : IStreamProcessorAggregateFactory
    {
        TFactory Factory { get; }
    }
}