using System;
using System.Linq;
using System.Threading.Tasks;
using Orleans.Placement;
using Orleans.Streams.Messages;

namespace Orleans.Streams.Linq.Nodes
{
    /// <summary>
    ///     Executes select operation on a stream and forwards it to its output stream.
    /// </summary>
    [PreferLocalPlacement]
    internal class StreamProcessorSelectNodeGrain<TIn, TOut> : StreamProcessorNodeGrain<TIn, TOut>, IStreamProcessorSelectNodeGrain<TIn, TOut>
    {
        private Func<TIn, TOut> _function;

        /// <summary>
        ///     Set the select function.
        /// </summary>
        /// <param name="function">Selection function for each item.</param>
        /// <returns></returns>
        public Task SetFunction(SerializableFunc<TIn, TOut> function)
        {
            _function = function.Value.Compile();
            return TaskDone.Done;
        }

        protected override void RegisterMessages()
        {
            base.RegisterMessages();
            StreamConsumer.MessageDispatcher.Register<ItemMessage<TIn>>(ProcessItemAddMessage);
        }

        protected Task ProcessItemAddMessage(ItemMessage<TIn> itemMessage)
        {
            var result = itemMessage.Items.Select(_function).ToList();
            StreamSender.EnqueueMessage(new ItemMessage<TOut>(result));
            return TaskDone.Done;
        }
    }
}