using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Orleans.Streams.Linq.Aggregates
{
    /// <summary>
    ///     Transforms data from TIn to TOut using multiple IStreamProcessorNode.
    /// </summary>
    /// <typeparam name="TIn">Data input type.</typeparam>
    /// <typeparam name="TOut">Data output type.</typeparam>
    /// <typeparam name="TNode">Type of processor node.</typeparam>
    public abstract class StreamProcessorAggregate<TIn, TOut, TNode> : Grain, IStreamProcessorAggregate<TIn, TOut>
        where TNode : IStreamProcessorNodeGrain<TIn, TOut>
    {
        /// <summary>
        ///     Nodes used for processing the data.
        /// </summary>
        protected List<TNode> ProcessorNodes = new List<TNode>();

        /// <summary>
        ///     Set input source.
        /// </summary>
        /// <param name="streamIdentities">Information about streams to subscribe to.</param>
        /// <returns></returns>
        public async Task SetInput(IList<StreamIdentity> streamIdentities)
        {
            ProcessorNodes = new List<TNode>();
            foreach (var identity in streamIdentities)
            {
                var node = await InitializeNode(identity);

                ProcessorNodes.Add(node);
            }
        }

        /// <summary>
        ///     Wait for a transaction to be completed.
        /// </summary>
        /// <param name="transactionId">Transaction id to await.</param>
        /// <returns></returns>
        public async Task TransactionComplete(Guid transactionId)
        {
            await Task.WhenAll(ProcessorNodes.Select(p => p.TransactionComplete(transactionId)));
        }

        /// <summary>
        ///     Get identities of the provided output streams.
        /// </summary>
        /// <returns></returns>
        public async Task<IList<StreamIdentity>> GetOutputStreams()
        {
            var result = await Task.WhenAll(ProcessorNodes.Select(n => n.GetOutputStreams()));

            var resultingStreams = result.SelectMany(s => s).ToList();
            return resultingStreams;
        }

        /// <summary>
        ///     Checks if this entity is teared down.
        /// </summary>
        /// <returns></returns>
        public async Task<bool> IsTearedDown()
        {
            var tearDownStates = await Task.WhenAll(ProcessorNodes.Select(n => n.IsTearedDown()));

            return tearDownStates.All(x => x);
        }

        /// <summary>
        ///     Unsubscribes from all streams this entity consumes and remove references to stream this entity produces.
        /// </summary>
        /// <returns></returns>
        public async Task TearDown()
        {
            await Task.WhenAll(ProcessorNodes.Select(p => p.TearDown()));
        }

        /// <summary>
        ///     Operation to create a IStreamProcessorNodeGrain of type TNode.
        /// </summary>
        /// <param name="identity">Identity to subscribe the node to.</param>
        /// <returns></returns>
        protected abstract Task<TNode> InitializeNode(StreamIdentity identity);
    }
}