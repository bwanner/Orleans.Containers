using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Orleans.Streams.Linq.Aggregates
{
    public class StreamProcessorWhereAggregate<TIn> : StreamProcessorAggregate<TIn, TIn>, IStreamProcessorWhereAggregate<TIn>
    {
        private Func<TIn, bool> _functionTemplate;

        public Task SetFunction(Func<TIn, bool> function)
        {
            _functionTemplate = function;
            return TaskDone.Done;
        }

        protected override async Task<IStreamProcessorNodeGrain<TIn, TIn>> InitializeNode(StreamIdentity<TIn> identity)
        {
            var node = GrainFactory.GetGrain<IStreamProcessorWhereNodeGrain<TIn>>(Guid.NewGuid());
            await node.SetFunction(_functionTemplate);
            await node.SetInput(identity);

            return node;
        }
    }
}