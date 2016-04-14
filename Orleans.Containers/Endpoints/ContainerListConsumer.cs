using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Orleans.Collections.Messages;
using Orleans.Streams;
using Orleans.Streams.Endpoints;

namespace Orleans.Collections.Endpoints
{
    public class ContainerElementListConsumer<T> : MultiStreamListConsumer<ContainerElement<T>>
    {
        public ContainerElementListConsumer(IStreamProvider streamProvider) : base(streamProvider)
        {
        }

        protected override void SetupMessageDispatcher(StreamMessageDispatchReceiver dispatcher)
        {
            base.SetupMessageDispatcher(dispatcher);
            dispatcher.Register<ItemUpdateMessage<ContainerElement<T>>>(ProcessItemUpdateMessage);
            dispatcher.Register<ItemRemoveMessage<ContainerElement<T>>>(ProcessItemRemoveMesssage);
        }

        private Task ProcessItemUpdateMessage(ItemUpdateMessage<ContainerElement<T>> message)
        {
            foreach (var item in message.Items)
            {
                var itemIndex = Items.FindIndex(element => element.Reference.Equals(item.Reference));
                if (itemIndex < 0)
                {
                    // ignore for now
                }
                else
                {
                    Items[itemIndex] = item;
                }
            }

            return TaskDone.Done;
        }

        private Task ProcessItemRemoveMesssage(ItemRemoveMessage<ContainerElement<T>> message)
        {
            foreach (var item in message.Items)
            {
                var itemIndex = Items.FindIndex(element => element.Reference.Equals(item.Reference));
                if (itemIndex < 0)
                {
                    // ignore for now
                }
                else
                {
                    Items.RemoveAt(itemIndex);
                }
            }

            return TaskDone.Done;
        }
    }
}