using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Orleans.Streams.Linq.Nodes
{
    internal class StreamProcessorSelectNodeGrain<TIn, TOut> : StreamProcessorNodeGrain<TIn, TOut>, IStreamProcessorSelectNodeGrain<TIn, TOut>
    {
        private Func<TIn, TOut> _function;

        public Task SetFunction(Func<TIn, TOut> function)
        {
            _function = function;
            return TaskDone.Done;
        }

        protected override async Task ItemArrived(IEnumerable<TIn> items)
        {
            var result = items.Select(_function).ToList();
            await StreamProvider.SendItems(result, false);
        }
    }
}