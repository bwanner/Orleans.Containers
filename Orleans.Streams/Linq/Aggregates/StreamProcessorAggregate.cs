using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Orleans.Streams.Linq.Aggregates
{
    public abstract class StreamProcessorAggregate<TIn, TOut> : Grain, IStreamProcessorAggregate<TIn, TOut>
    {
        protected List<IStreamProcessorNodeGrain<TIn, TOut>> ProcessorNodes;

        public async Task SetInput(IEnumerable<StreamIdentity> streamIdentities)
        {
            ProcessorNodes = new List<IStreamProcessorNodeGrain<TIn, TOut>>();
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

        protected abstract Task<IStreamProcessorNodeGrain<TIn, TOut>> InitializeNode(StreamIdentity identity);
        
        public async Task<IList<StreamIdentity>> GetStreamIdentities()
        {
            var result = await Task.WhenAll(ProcessorNodes.Select(n => n.GetStreamIdentity()));

            return result.ToList();
        }

        public async Task TearDown()
        {
            await Task.WhenAll(ProcessorNodes.Select(p => p.TearDown()));
        }

        public async Task<bool> IsTearedDown()
        {
            var tearDownStates = await Task.WhenAll(ProcessorNodes.Select(n => n.IsTearedDown()));

            return tearDownStates.All(x => x == true);
        }
    }
}