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
    /// <typeparam name="T">Data type transmitted via the stream.</typeparam>
    public class StreamMessageSender<T> : IStreamMessageSender<T>
    {
        private readonly List<T> _addItems = new List<T>();
        private readonly List<T> _removeItems = new List<T>();
        private readonly InternalStreamMessageSender _sender;
        private readonly List<T> _updateItems = new List<T>();

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="provider">Provider to use for stream.</param>
        /// <param name="guid">Identifier to use for the stream. Random Guid will be generated if default.</param>
        public StreamMessageSender(IStreamProvider provider, Guid guid = default(Guid))
        {
            _sender = new InternalStreamMessageSender(provider, guid);
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="provider">Provider to use for stream.</param>
        /// <param name="targetStream">Identity of the stream to create.</param>
        public StreamMessageSender(IStreamProvider provider, StreamIdentity targetStream)
        {
            _sender = new InternalStreamMessageSender(provider, targetStream);
        }

        /// <summary>
        /// End a transaction.
        /// </summary>
        /// <param name="transactionId">Transaction identifier.</param>
        /// <returns></returns>
        public async Task EndTransaction(Guid transactionId)
        {
            await _sender.EndTransaction(transactionId);
        }

        /// <summary>
        /// Enqueue a AddItemMessage for all items.
        /// </summary>
        /// <param name="items">Items to send.</param>
        /// <returns></returns>
        public void EnqueueAddItems(IEnumerable<T> items)
        {
            _addItems.AddRange(items);
        }

        /// <summary>
        /// Enqeue a generic message.
        /// </summary>
        /// <returns></returns>
        public void EnqueueMessage(IStreamMessage message)
        {
            _sender.AddToMessageQueue(message);
        }

        /// <summary>
        /// Enqueue a RemoveItemMessage for all items.
        /// </summary>
        /// <param name="items">Items to send.</param>
        /// <returns></returns>
        public void EnqueueRemoveItems(IEnumerable<T> items)
        {
            _removeItems.AddRange(items);
        }

        /// <summary>
        /// Enqueue a UpdateItemMessage for all items.
        /// </summary>
        /// <param name="items">Items to send.</param>
        /// <returns></returns>
        public void EnqueueUpdateItems(IEnumerable<T> items)
        {
            _updateItems.AddRange(items);
        }

        /// <summary>
        /// Send all messages and empty the queue.
        /// </summary>
        /// <returns></returns>
        public async Task FlushQueue()
        {
            if (_addItems.Count > 0)
                await _sender.SendMessage(new ItemAddMessage<T>(_addItems));
            if (_updateItems.Count > 0)
                await _sender.SendMessage(new ItemUpdateMessage<T>(_updateItems));
            if (_removeItems.Count > 0)
                await _sender.SendMessage(new ItemRemoveMessage<T>(_removeItems));

            _addItems.Clear();
            _updateItems.Clear();
            _removeItems.Clear();

            await _sender.SendMessagesFromQueue();
        }

        /// <summary>
        /// Send a AddItemMessage for all items.
        /// </summary>
        /// <param name="items">Items to send.</param>
        /// <returns></returns>
        public async Task SendAddItems(IEnumerable<T> items)
        {
            await _sender.SendMessage(new ItemAddMessage<T>(items));
        }

        /// <summary>
        /// Send items via this strem.
        /// </summary>
        /// <param name="items">Items to send.</param>
        /// <returns></returns>
        public async Task SendItems(IEnumerable<T> items)
        {
            var message = new ItemAddMessage<T>(items);
            await Task.WhenAll(_sender.SendMessage(message));
        }

        /// <summary>
        /// Send a RemoveItemMessage for all items.
        /// </summary>
        /// <param name="items">Items to send.</param>
        /// <returns></returns>
        public async Task SendRemoveItems(IEnumerable<T> items)
        {
            await _sender.SendMessage(new ItemRemoveMessage<T>(items));
        }

        /// <summary>
        /// Send a UpdateItemMessage for all items.
        /// </summary>
        /// <param name="items">Items to send.</param>
        /// <returns></returns>
        public async Task SendUpdateItems(IEnumerable<T> items)
        {
            await _sender.SendMessage(new ItemUpdateMessage<T>(items));
        }

        /// <summary>
        /// Start a transaction.
        /// </summary>
        /// <param name="transactionId">Transaction identifier.</param>
        /// <returns></returns>
        public async Task StartTransaction(Guid transactionId)
        {
            await _sender.StartTransaction(transactionId);
        }

        /// <summary>
        /// Get identities of the provided output streams.
        /// </summary>
        /// <returns></returns>
        public async Task<IList<StreamIdentity>> GetOutputStreams()
        {
            return (await _sender.GetStreamIdentity()).SingleValueToList();
        }

        /// <summary>
        /// Checks if this output stream is teared down.
        /// </summary>
        /// <returns></returns>
        public Task<bool> IsTearedDown()
        {
            return _sender.IsTearedDown();
        }

        /// <summary>
        /// Tears down the output stream.
        /// </summary>
        /// <returns></returns>
        public Task TearDown()
        {
            return _sender.TearDown();
        }

        /// <summary>
        ///     Sends a generic message.
        /// </summary>
        /// <param name="message">Message to send.</param>
        /// <returns></returns>
        public async Task SendMessage(IStreamMessage message)
        {
            await _sender.SendMessage(message);
        }
    }
}