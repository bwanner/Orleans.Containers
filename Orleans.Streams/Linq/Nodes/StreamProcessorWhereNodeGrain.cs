using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Orleans.Streams.Messages;

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

        public override async Task Visit(ItemMessage<TIn> itemMessage)
        {
            var resultList = itemMessage.Items.Where(item => _function(item)).ToList();
            await StreamProvider.SendItems(resultList, false);
        }
    }
}