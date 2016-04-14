using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Orleans.Collections.Messages;
using Orleans.Streams.Messages;

namespace Orleans.Streams.Endpoints
{
    /// <summary>
    /// Consumes items from multiple streams.
    /// </summary>
    /// <typeparam name="T">Type of items to consume.</typeparam>
    public class MultiStreamConsumer<T> : ITransactionalStreamConsumerAggregate
    {
        private readonly IStreamProvider _streamProvider;
        protected readonly List<SingleStreamTransactionReceiver> TransactionManagers;
        protected readonly List<StreamMessageDispatchReceiver> MessageDispatchers; 
        private bool _tearDownExecuted;
        protected Func<IEnumerable<T>, Task> StreamItemBatchReceivedFunc;
        protected Func<TransactionMessage, Task> StreamTransactionReceivedFunc;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="streamProvider">Stream provider to be used.</param>
        /// <param name="streamItemBatchReceivedFunc">Asynchronous function to be executed when an item is received.</param>
        /// <param name="streamTransactionReceivedFunc">Asynchronous function to be executed when a transaction message is received.</param>
        public MultiStreamConsumer(IStreamProvider streamProvider, Func<IEnumerable<T>, Task> streamItemBatchReceivedFunc,
            Func<TransactionMessage, Task> streamTransactionReceivedFunc = null)
        {
            TransactionManagers = new List<SingleStreamTransactionReceiver>();
            MessageDispatchers = new List<StreamMessageDispatchReceiver>();
            _streamProvider = streamProvider;
            StreamTransactionReceivedFunc = streamTransactionReceivedFunc;
            StreamItemBatchReceivedFunc = streamItemBatchReceivedFunc;
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="streamProvider">Stream provider to be used.</param>
        /// <param name="streamItemBatchReceivedAction">Action to be executed when an item is received.</param>
        /// <param name="streamTransactionReceivedAction">Action to be executed when a transaction message is received.</param>
        public MultiStreamConsumer(IStreamProvider streamProvider, Action<IEnumerable<T>> streamItemBatchReceivedAction = null,
            Action<TransactionMessage> streamTransactionReceivedAction = null)
        {
            TransactionManagers = new List<SingleStreamTransactionReceiver>();
            MessageDispatchers = new List<StreamMessageDispatchReceiver>();
            _streamProvider = streamProvider;

            if (streamTransactionReceivedAction != null)
            {
                StreamTransactionReceivedFunc = transaction =>
                {
                    streamTransactionReceivedAction(transaction);
                    return TaskDone.Done;
                };
            }

            if (streamItemBatchReceivedAction != null)
            {
                StreamItemBatchReceivedFunc = items =>
                {
                    streamItemBatchReceivedAction(items);
                    return TaskDone.Done;
                };
            }
            ;
        }

        public async Task SetInput(IEnumerable<StreamIdentity> streamIdentities)
        {
            _tearDownExecuted = false;
            foreach (var identity in streamIdentities)
            {
                var dispatcher = new StreamMessageDispatchReceiver(_streamProvider, null);
                var consumer = new SingleStreamTransactionReceiver(dispatcher);

                await dispatcher.Subscribe(identity);
                SetupMessageDispatcher(dispatcher);

                MessageDispatchers.Add(dispatcher);
                TransactionManagers.Add(consumer);
            }
        }

        protected virtual void SetupMessageDispatcher(StreamMessageDispatchReceiver dispatcher)
        {
            dispatcher.Register<ItemAddMessage<T>>(ProcessItemMessage);
            dispatcher.Register<TransactionMessage>(ProcessTransactionMessage);
        }

        /// <summary>
        /// Returns when a transaction is complete.
        /// </summary>
        /// <param name="transactionId">Transaction identifier.</param>
        /// <returns></returns>
        public async Task TransactionComplete(Guid transactionId)
        {
            await Task.WhenAll(TransactionManagers.Select(c => c.TransactionComplete(transactionId)));
        }

        /// <summary>
        /// Returns true if consumer is teared down.
        /// </summary>
        /// <returns></returns>
        public Task<bool> IsTearedDown()
        {
            return Task.FromResult(_tearDownExecuted);
        }

        public virtual async Task TearDown()
        {
            await Task.WhenAll(MessageDispatchers.Select(c => c.TearDown()));
            _tearDownExecuted = true;
        }

        public async Task ProcessItemMessage(ItemAddMessage<T> message)
        {
            if (StreamItemBatchReceivedFunc != null)
            {
                await StreamItemBatchReceivedFunc(message.Items);
            }
        }

        public async Task ProcessTransactionMessage(TransactionMessage transactionMessage)
        {
            if (StreamTransactionReceivedFunc != null)
            {
                await StreamTransactionReceivedFunc(transactionMessage);
            }
        }
    }
}