using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Orleans.Streams.Partitioning;

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
            var siloIdentitiesWithLocation = streamIdentities.Select(s => new SiloLocationStreamIdentity(s.Guid, s.Namespace, "")).ToList();
            await SetInput(siloIdentitiesWithLocation);
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

        public async Task<IList<SiloLocationStreamIdentity>> GetOutputStreamsWithSourceLocation()
        {
            var result = await Task.WhenAll(ProcessorNodes.Select(n => n.GetOutputStreamsWithSourceLocation()));

            var resultingStreams = result.SelectMany(s => s).ToList();
            return resultingStreams;
        }

        public async Task SetInput(IList<SiloLocationStreamIdentity> streamIdentities)
        {
            var processorStreamPairs = await CreateProcessorNodes(streamIdentities);
            ProcessorNodes = processorStreamPairs.Select(tuple => tuple.Item1).ToList();

            var createdNodes = await Task.WhenAll(processorStreamPairs.Select(InitializeNode).ToList());
        }

        private async Task<List<Tuple<TNode, StreamIdentity>>> CreateProcessorNodes(IList<SiloLocationStreamIdentity> inputStreams)
        {
            var nodes = new List<Tuple<TNode, StreamIdentity>>();
            var availableSilos = await PartitionGrainUtil.GetExecutionGrains(GrainFactory);

            var silosToBeUsed = availableSilos.Repeat().Take(inputStreams.Count).ToList();

            // Greedy match with fixed stream
            foreach(var inputStream in inputStreams)
            {
                var localContexts = silosToBeUsed.Where(tuple => tuple.Item2 == inputStream.Silo);
                Tuple<ISiloContextExecutionGrain, string> selectedContext = null;

                selectedContext = localContexts.Any() ? localContexts.First() : silosToBeUsed[0];

                var nodeGrain = (TNode) await selectedContext.Item1.ExecuteFunc(async (factory, state) =>
                {
                    var grain = factory.GetGrain<TNode>(Guid.NewGuid());
                    var stream = (SiloLocationStreamIdentity) state; // TODO call setup method here? await grain.SubscribeToStreams(stream.SingleValueToList());
                    await grain.IsTearedDown(); // Call this so the grain is activated.
                    return grain;
                }, inputStream);
                    nodes.Add(new Tuple<TNode, StreamIdentity>(nodeGrain, inputStream));
                    silosToBeUsed.Remove(selectedContext);
            }

            return nodes;
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
        ///     Operation to initialize a IStreamProcessorNodeGrain of type TNode.
        /// </summary>
        /// <param name="nodeStreamPair"></param>
        /// <returns></returns>
        protected abstract Task<TNode> InitializeNode(Tuple<TNode, StreamIdentity> nodeStreamPair);
    }
}