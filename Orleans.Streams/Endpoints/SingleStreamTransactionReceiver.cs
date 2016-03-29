using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Orleans.Streams.Messages;

namespace Orleans.Streams.Endpoints
{
    /// <summary>
    /// Consumes items of a single stream.
    /// </summary>
    public class SingleStreamTransactionReceiver
    {
        private readonly Dictionary<Guid, TaskCompletionSource<Task>> _awaitedTransactions;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="dispatchReceiver">Dispatcher used to subscribe to transaction message.</param>
        public SingleStreamTransactionReceiver(StreamMessageDispatchReceiver dispatchReceiver)
        {
            _awaitedTransactions = new Dictionary<Guid, TaskCompletionSource<Task>>();
            dispatchReceiver.Register<TransactionMessage>(ProcessTransactionMessage);
        }

        /// <summary>
        /// Returns if transaction is completed.
        /// </summary>
        /// <param name="transactionId">Transaction identifier.</param>
        /// <returns></returns>
        public async Task TransactionComplete(Guid transactionId)
        {
            if (!_awaitedTransactions.ContainsKey(transactionId))
            {
                _awaitedTransactions[transactionId] = new TaskCompletionSource<Task>();
            }

            await _awaitedTransactions[transactionId].Task;
        }

        /// <summary>
        /// Process a transaction message.
        /// </summary>
        /// <param name="transactionMessage"></param>
        private Task ProcessTransactionMessage(TransactionMessage transactionMessage)
        {
            if (transactionMessage.State == TransactionState.Start)
            {
                if (!_awaitedTransactions.ContainsKey(transactionMessage.TransactionId))
                {
                    _awaitedTransactions[transactionMessage.TransactionId] = new TaskCompletionSource<Task>();
                }
            }

            else if (transactionMessage.State == TransactionState.End)
            {
                _awaitedTransactions[transactionMessage.TransactionId].SetResult(TaskDone.Done);
            }

            return TaskDone.Done;
        }
    }
}