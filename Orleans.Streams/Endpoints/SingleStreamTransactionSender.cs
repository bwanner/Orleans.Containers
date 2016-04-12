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
        private readonly StreamMessageSender _sender;

        public StreamMessageSender Sender => _sender;

        public SingleStreamTransactionSender(StreamMessageSender sender)
        {
            _sender = sender;
        }

        public void EnqueueItemsForSending(params T[] items)
        {
            var message = new ItemMessage<T>(items);
            _sender.AddToMessageQueue(message);
        }

        public async Task<Guid> SendItems(IEnumerable<T> items, bool useTransaction = true, Guid? transactionId = null)
        {
            var curTransactionId = transactionId ?? Guid.NewGuid();
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

        public Task<Guid> SendItem(T item, bool useTransaction = true, Guid? transactionId = null)
        {
            return SendItems(new List<T>(1) {item}, useTransaction, transactionId);
        }

        public async Task StartTransaction(Guid transactionId)
        {
            await _sender.SendMessage(new TransactionMessage {State = TransactionState.Start, TransactionId = transactionId});
        }

        public async Task EndTransaction(Guid transactionId)
        {
            await _sender.SendMessage(new TransactionMessage {State = TransactionState.End, TransactionId = transactionId});
        }
    }
}