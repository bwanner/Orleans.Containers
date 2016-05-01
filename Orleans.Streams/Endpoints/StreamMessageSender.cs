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
    public class StreamMessageSender<T> : IStreamMessageSender<T>
    {
        private readonly List<T> _addItems = new List<T>();
        private readonly List<T> _updateItems = new List<T>();
        private readonly List<T> _removeItems = new List<T>();
        private readonly InternalStreamMessageSender _sender;

        public StreamMessageSender(IStreamProvider provider, Guid guid = default(Guid))
        {
            _sender = new InternalStreamMessageSender(provider, guid);
        }

        public StreamMessageSender(IStreamProvider provider, StreamIdentity targetStream)
        {
            _sender = new InternalStreamMessageSender(provider, targetStream);
        }

        public async Task SendItems(IEnumerable<T> items)
        {
            var message = new ItemAddMessage<T>(items);
            await Task.WhenAll(_sender.SendMessage(message));
        }

        public async Task StartTransaction(Guid transactionId)
        {
            await _sender.StartTransaction(transactionId);
        }

        public async Task EndTransaction(Guid transactionId)
        {
            await _sender.EndTransaction(transactionId);
        }

        public async Task SendAddItems(IEnumerable<T> items)
        {
            await _sender.SendMessage(new ItemAddMessage<T>(items));
        }

        public async Task SendUpdateItems(IEnumerable<T> items)
        {
            await _sender.SendMessage(new ItemUpdateMessage<T>(items));
        }

        public async Task SendRemoveItems(IEnumerable<T> items)
        {
            await _sender.SendMessage(new ItemRemoveMessage<T>(items));
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
            _sender.AddToMessageQueue(message);
        }

        public async Task FlushQueue()
        {
            if(_addItems.Count > 0)
                await _sender.SendMessage(new ItemAddMessage<T>(_addItems));
            if(_updateItems.Count > 0)
                await _sender.SendMessage(new ItemUpdateMessage<T>(_updateItems));
            if(_removeItems.Count > 0)
                await _sender.SendMessage(new ItemRemoveMessage<T>(_removeItems));

            _addItems.Clear();
            _updateItems.Clear();
            _removeItems.Clear();

            await _sender.SendMessagesFromQueue();
        }

        public async Task<IList<StreamIdentity>> GetOutputStreams()
        {
            return (await _sender.GetStreamIdentity()).SingleValueToList();
        }

        public Task TearDown()
        {
            return _sender.TearDown();
        }

        public Task<bool> IsTearedDown()
        {
            return _sender.IsTearedDown();
        }
    }
}