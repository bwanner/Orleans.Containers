using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Orleans.Streams.Linq.Nodes
{
    internal class StreamProcessorWhereNodeGrain<TIn> : StreamProcessorNodeGrain<TIn, TIn>, IStreamProcessorWhereNodeGrain<TIn>
    {
        private Func<TIn, bool> _function;

        public Task SetFunction(Func<TIn, bool> function)
        {
            _function = function;
            return TaskDone.Done;
        }

        protected override async Task ItemArrived(IEnumerable<TIn> items)
        {
            var resultList = items.Where(item => _function(item)).ToList();
            await StreamProvider.SendItems(resultList, false);
        }
    }
}