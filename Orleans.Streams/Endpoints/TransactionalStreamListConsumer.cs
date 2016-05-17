using System.Collections.Generic;
using System.Threading.Tasks;
using Orleans.Streams.Messages;

namespace Orleans.Streams.Endpoints
{
    /// <summary>
    ///     Consumes items from multiple streams and places them in a list.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class TransactionalStreamListConsumer<T> : TransactionalStreamConsumer
    {
        /// <summary>
        ///     List of stored items.
        /// </summary>
        public List<T> Items { get; }

        /// <summary>
        ///     Constructor.
        /// </summary>
        /// <param name="streamProvider">Stream provider to use.</param>
        public TransactionalStreamListConsumer(IStreamProvider streamProvider) : base(streamProvider, null)
        {
            Items = new List<T>();
        }

        protected override void SetupMessageDispatcher(StreamMessageDispatchReceiver dispatcher)
        {
            base.SetupMessageDispatcher(dispatcher);
            dispatcher.Register<ItemMessage<T>>(ProcessItemAddMessage);
        }

        private Task ProcessItemAddMessage(ItemMessage<T> message)
        {
            Items.AddRange(message.Items);
            return TaskDone.Done;
        }
    }
}