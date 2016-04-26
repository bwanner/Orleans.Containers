using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Orleans.Streams.Linq.Aggregates
{
    public abstract class StreamProcessorAggregate<TIn, TOut, TNode> : Grain, IStreamProcessorAggregate<TIn, TOut>
        where TNode : IStreamProcessorNodeGrain<TIn, TOut>
    {
        protected List<TNode> ProcessorNodes;

        public async Task SetInput(IList<StreamIdentity> streamIdentities)
        {
            ProcessorNodes = new List<TNode>();
            foreach (var identity in streamIdentities)
            {
                var node = await InitializeNode(identity);

                ProcessorNodes.Add(node);
            }
        }

        public async Task TransactionComplete(Guid transactionId)
        {
            await Task.WhenAll(ProcessorNodes.Select(p => p.TransactionComplete(transactionId)));
        }

        public async Task<IList<StreamIdentity>> GetStreamIdentities()
        {
            var result = await Task.WhenAll(ProcessorNodes.Select(n => n.GetOutputStreams()));

            var resultingStreams = result.SelectMany(s => s).ToList();
            return resultingStreams;
        }

        public async Task<bool> IsTearedDown()
        {
            var tearDownStates = await Task.WhenAll(ProcessorNodes.Select(n => n.IsTearedDown()));

            return tearDownStates.All(x => x);
        }

        public async Task TearDown()
        {
            await Task.WhenAll(ProcessorNodes.Select(p => p.TearDown()));
        }

        protected abstract Task<TNode> InitializeNode(StreamIdentity identity);
    }
}