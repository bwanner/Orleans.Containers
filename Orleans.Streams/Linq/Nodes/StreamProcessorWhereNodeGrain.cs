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

        public Task SetFunction(SerializableFunc<TIn, bool> function)
        {
            _function = function.Value.Compile();
            return TaskDone.Done;
        }

        protected override async Task ProcessItemMessage(ItemMessage<TIn> itemMessage)
        {
            var resultList = itemMessage.Items.Where(item => _function(item)).ToList();
            await StreamTransactionSender.SendItems(resultList, false);
        }
    }
}