using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Orleans.Collections.Messages;
using Orleans.Streams.Messages;

namespace Orleans.Streams.Linq.Nodes
{
    internal class StreamProcessorWhereNodeGrain<TIn> : StreamProcessorNodeGrain<TIn, TIn>, IStreamProcessorWhereNodeGrain<TIn>
    {
        private Func<TIn, bool> _function;

        protected override void RegisterMessages()
        {
            base.RegisterMessages();
            StreamMessageDispatchReceiver.Register<ItemAddMessage<TIn>>(ProcessItemAddMessage);
        }

        public Task SetFunction(SerializableFunc<TIn, bool> function)
        {
            _function = function.Value.Compile();
            return TaskDone.Done;
        }

        protected async Task ProcessItemAddMessage(ItemAddMessage<TIn> itemMessage)
        {
            var resultList = itemMessage.Items.Where(item => _function(item)).ToList();
            await StreamSender.SendItems(resultList);
        }
    }
}