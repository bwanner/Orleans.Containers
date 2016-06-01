using System;
using System.Linq;
using System.Threading.Tasks;
using Orleans.Placement;
using Orleans.Streams.Messages;

namespace Orleans.Streams.Linq.Nodes
{
    /// <summary>
    ///     Executes where operations on a stream and forwards results evaluating to 'true' to its output stream.
    /// </summary>
    [PreferLocalPlacement]
    internal class StreamProcessorWhereNodeGrain<TIn> : StreamProcessorNodeGrain<TIn, TIn>, IStreamProcessorWhereNodeGrain<TIn>
    {
        private Func<TIn, bool> _function;

        /// <summary>
        /// Set the where function.
        /// </summary>
        /// <param name="function">Filter function for each item. Evaluation to 'true' will forward results to the output stream.</param>
        /// <returns></returns>
        public Task SetFunction(SerializableFunc<TIn, bool> function)
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
            var resultList = itemMessage.Items.Where(item => _function(item)).ToList();
            if(resultList.Count > 0)
                StreamSender.EnqueueMessage(new ItemMessage<TIn>(resultList));

            return TaskDone.Done;
        }
    }
}