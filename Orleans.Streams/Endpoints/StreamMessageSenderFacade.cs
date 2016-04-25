using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Orleans.Collections.Messages;
using Orleans.Streams.Messages;

namespace Orleans.Streams.Endpoints
{
    /// <summary>
    ///     Makes default stream operations available via its interface.
    /// </summary>
    /// <typeparam name="T">Data type transmitted via the stream</typeparam>
    public class StreamMessageSenderFacade<T> : ITransactionalStreamTearDown, ITransactionalStreamProvider
    {
        private readonly List<T> _addItems = new List<T>();
        private readonly List<T> _updateItems = new List<T>();
        private readonly List<T> _removeItems = new List<T>();
        protected readonly StreamMessageSender Sender;

        public StreamMessageSenderFacade(StreamMessageSender sender)
        {
            Sender = sender;
        }

        // TODO remove
        public async Task SendItems(IEnumerable<T> items)
        {
            var message = new ItemAddMessage<T>(items);
            await Task.WhenAll(Sender.SendMessage(message));
        }

        public async Task StartTransaction(Guid transactionId)
        {
            await Sender.StartTransaction(transactionId);
        }

        public async Task EndTransaction(Guid transactionId)
        {
            await Sender.EndTransaction(transactionId);
        }

        public async Task SendAddItems(IEnumerable<T> items)
        {
            await Sender.SendMessage(new ItemAddMessage<T>(items));
        }

        public async Task SendUpdateItems(IEnumerable<T> items)
        {
            await Sender.SendMessage(new ItemUpdateMessage<T>(items));
        }

        public async Task SendRemoveItems(IEnumerable<T> items)
        {
            await Sender.SendMessage(new ItemRemoveMessage<T>(items));
        }

        public void EnqueueAddItems(IEnumerable<T> items)
        {
            _addItems.AddRange(items);
        }

        public void EnqueueUpdateItems(IEnumerable<T> items)
        {
            _updateItems.AddRange(items);
        }

        public void EnqueueRemoveItems(IEnumerable<T> items)
        {
            _removeItems.AddRange(items);
        }

        public void EnqueueMessage(IStreamMessage message)
        {
            Sender.AddToMessageQueue(message);
        }

        public async Task FlushQueue()
        {
            await Sender.SendMessage(new ItemAddMessage<T>(_addItems));
            await Sender.SendMessage(new ItemUpdateMessage<T>(_updateItems));
            await Sender.SendMessage(new ItemRemoveMessage<T>(_removeItems));

            _addItems.Clear();
            _updateItems.Clear();
            _removeItems.Clear();

            await Sender.SendMessagesFromQueue();
        }

        public Task TearDown()
        {
            return Sender.TearDown();
        }

        public Task<bool> IsTearedDown()
        {
            return Sender.IsTearedDown();
        }

        public Task<StreamIdentity> GetStreamIdentity()
        {
            return ((ITransactionalStreamProvider) Sender).GetStreamIdentity();
        }
    }
}