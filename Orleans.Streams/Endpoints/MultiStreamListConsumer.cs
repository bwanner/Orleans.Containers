using System.Collections.Generic;
using System.Threading.Tasks;
using Orleans.Collections.Messages;

namespace Orleans.Streams.Endpoints
{
    /// <summary>
    ///     Consumes items from multiple streams and places them in a list.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class MultiStreamListConsumer<T> : MultiStreamConsumer<T>
    {
        /// <summary>
        ///     List of stored items.
        /// </summary>
        public List<T> Items { get; }

        /// <summary>
        ///     Constructor.
        /// </summary>
        /// <param name="streamProvider">Stream provider to use.</param>
        public MultiStreamListConsumer(IStreamProvider streamProvider) : base(streamProvider)
        {
            Items = new List<T>();
        }

        protected override void SetupMessageDispatcher(StreamMessageDispatchReceiver dispatcher)
        {
            base.SetupMessageDispatcher(dispatcher);
            dispatcher.Register<ItemAddMessage<T>>(ProcessItemAddMessage);
        }

        private Task ProcessItemAddMessage(ItemAddMessage<T> message)
        {
            Items.AddRange(message.Items);
            return TaskDone.Done;
        }
    }
}