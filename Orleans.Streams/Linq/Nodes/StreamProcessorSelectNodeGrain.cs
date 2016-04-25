using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Orleans.Collections.Messages;
using Orleans.Streams.Messages;

namespace Orleans.Streams.Linq.Nodes
{
    internal class StreamProcessorSelectNodeGrain<TIn, TOut> : StreamProcessorNodeGrain<TIn, TOut>, IStreamProcessorSelectNodeGrain<TIn, TOut>
    {
        private Func<TIn, TOut> _function;

        public override Task OnActivateAsync()
        {
            return base.OnActivateAsync();
        }

        protected override void RegisterMessages()
        {
            base.RegisterMessages();
            StreamMessageDispatchReceiver.Register<ItemAddMessage<TIn>>(ProcessItemAddMessage);
        }

        public Task SetFunction(SerializableFunc<TIn, TOut> function)
        {
            _function = function.Value.Compile();
            return TaskDone.Done;
        }

        protected async Task ProcessItemAddMessage(ItemAddMessage<TIn> itemMessage)
        {
            var result = itemMessage.Items.Select(_function).ToList();
            await StreamSender.SendItems(result);
        }
    }
}