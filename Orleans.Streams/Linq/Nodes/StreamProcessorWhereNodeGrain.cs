using System;
using System.Linq;
using System.Threading.Tasks;
using Orleans.Collections.Messages;

namespace Orleans.Streams.Linq.Nodes
{
    /// <summary>
    ///     Executes where operations on a stream and forwards results evaluating to 'true' to its output stream.
    /// </summary>
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
            StreamConsumer.MessageDispatcher.Register<ItemAddMessage<TIn>>(ProcessItemAddMessage);
        }

        protected async Task ProcessItemAddMessage(ItemAddMessage<TIn> itemMessage)
        {
            var resultList = itemMessage.Items.Where(item => _function(item)).ToList();
            await StreamSender.SendItems(resultList);
        }
    }
}