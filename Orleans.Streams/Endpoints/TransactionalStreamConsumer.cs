using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Orleans.Streams.Endpoints
{
    /// <summary>
    ///     Consumes messages from multiple streams.
    /// </summary>
    public class TransactionalStreamConsumer : ITransactionalStreamConsumerAggregate
    {
        public StreamMessageDispatchReceiver MessageDispatcher { get; private set; }
        protected StreamTransactionReceiver TransactionReceiver;
        private bool _tearDownExecuted;

        /// <summary>
        ///     Constructor.
        /// </summary>
        /// <param name="streamProvider">Stream provider to be used.</param>
        public TransactionalStreamConsumer(IStreamProvider streamProvider, Func<Task> tearDownFunc = null)
        {
            MessageDispatcher = new StreamMessageDispatchReceiver(streamProvider, null, tearDownFunc);
            TransactionReceiver = new StreamTransactionReceiver(MessageDispatcher);
            // ReSharper disable once VirtualMemberCallInConstructor
            SetupMessageDispatcher(MessageDispatcher);
        }

        public async Task SetInput(IList<StreamIdentity> streamIdentities)
        {
            // TODO remove old subscriptions
            _tearDownExecuted = false;
            foreach (var identity in streamIdentities)
            {
                await MessageDispatcher.Subscribe(identity);
            }

        }

        /// <summary>
        ///     Returns when a transaction is complete.
        /// </summary>
        /// <param name="transactionId">Transaction identifier.</param>
        /// <returns></returns>
        public async Task TransactionComplete(Guid transactionId)
        {
            await TransactionReceiver.TransactionComplete(transactionId);
        }

        /// <summary>
        ///     Returns true if consumer is teared down.
        /// </summary>
        /// <returns></returns>
        public Task<bool> IsTearedDown()
        {
            return Task.FromResult(_tearDownExecuted);
        }

        public virtual async Task TearDown()
        {
            await MessageDispatcher.TearDown();
            _tearDownExecuted = true;
        }

        protected virtual void SetupMessageDispatcher(StreamMessageDispatchReceiver dispatcher)
        {
        }
    }
}