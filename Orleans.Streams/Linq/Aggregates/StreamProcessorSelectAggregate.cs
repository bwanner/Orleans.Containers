using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Orleans.Streams.Linq.Aggregates
{
    public class StreamProcessorSelectAggregate<TIn, TOut> : StreamProcessorAggregate<TIn, TOut>, IStreamProcessorSelectAggregate<TIn, TOut>
    {
        private SerializableFunc<TIn, TOut> _functionTemplate;

        public Task SetFunction(SerializableFunc<TIn, TOut> function)
        {
            _functionTemplate = function;
            return TaskDone.Done;
        }

        protected override async Task<IStreamProcessorNodeGrain<TIn, TOut>> InitializeNode(StreamIdentity identity)
        {
            var node = GrainFactory.GetGrain<IStreamProcessorSelectNodeGrain<TIn, TOut>>(Guid.NewGuid());
            await node.SetFunction(_functionTemplate);
            await node.SetInput(identity);

            return node;
        }

    }
}