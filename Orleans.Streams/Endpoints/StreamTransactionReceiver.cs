using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Orleans.Streams.Messages;

namespace Orleans.Streams.Endpoints
{
    /// <summary>
    ///     Keeps state of received transaction IDs.
    /// </summary>
    public class StreamTransactionReceiver
    {
        private readonly Dictionary<Guid, TaskCompletionSource<Task>> _awaitedTransactions;
        private readonly Dictionary<Guid, int> _awaitedTransactionCounter;

        /// <summary>
        ///     Constructor.
        /// </summary>
        /// <param name="dispatchReceiver">Dispatcher used to subscribe to transaction message.</param>
        public StreamTransactionReceiver(StreamMessageDispatchReceiver dispatchReceiver)
        {
            _awaitedTransactions = new Dictionary<Guid, TaskCompletionSource<Task>>();
            _awaitedTransactionCounter = new Dictionary<Guid, int>();
            dispatchReceiver.Register<TransactionMessage>(ProcessTransactionMessage);
        }

        /// <summary>
        ///     Returns if transaction is completed.
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
        ///     Process a transaction message.
        /// </summary>
        /// <param name="transactionMessage"></param>
        private Task ProcessTransactionMessage(TransactionMessage transactionMessage)
        {
            if (transactionMessage.State == TransactionState.Start)
            {
                if (!_awaitedTransactions.ContainsKey(transactionMessage.TransactionId))
                {
                    _awaitedTransactions[transactionMessage.TransactionId] = new TaskCompletionSource<Task>();
                    _awaitedTransactionCounter[transactionMessage.TransactionId] = 1;
                }

                else
                    _awaitedTransactionCounter[transactionMessage.TransactionId]++;
            }

            else if (transactionMessage.State == TransactionState.End)
            {
                if(--_awaitedTransactionCounter[transactionMessage.TransactionId] == 0)
                    _awaitedTransactions[transactionMessage.TransactionId].SetResult(TaskDone.Done);
            }

            return TaskDone.Done;
        }
    }
}