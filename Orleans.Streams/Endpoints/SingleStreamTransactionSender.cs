using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Orleans.Streams.Messages;

namespace Orleans.Streams.Endpoints
{
    /// <summary>
    ///     Provides a single transactional stream.
    /// </summary>
    /// <typeparam name="T">Data type transmitted via the stream</typeparam>
    public class SingleStreamTransactionSender<T>
    {
        private int _lastTransactionId = -1;
        private readonly StreamMessageSender _sender;

        public SingleStreamTransactionSender(StreamMessageSender sender)
        {
            _sender = sender;
        }

        public async Task<int> SendItems(IEnumerable<T> items, bool useTransaction = true, int? transactionId = null)
        {
            var curTransactionId = transactionId ?? ++_lastTransactionId;
            if (useTransaction)
            {
                await StartTransaction(curTransactionId);
            }
            var message = new ItemMessage<T>(items);
            await Task.WhenAll(_sender.SendMessage(message));
            if (useTransaction)
            {
                await EndTransaction(curTransactionId);
            }

            return curTransactionId;
        }

        public Task<int> SendItem(T item, bool useTransaction = true, int? transactionId = null)
        {
            return SendItems(new List<T>(1) {item}, useTransaction, transactionId);
        }

        public async Task StartTransaction(int transactionId)
        {
            await _sender.SendMessage(new TransactionMessage {State = TransactionState.Start, TransactionId = transactionId});
        }

        public async Task EndTransaction(int transactionId)
        {
            await _sender.SendMessage(new TransactionMessage {State = TransactionState.End, TransactionId = transactionId});
        }
    }
}