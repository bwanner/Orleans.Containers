using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Orleans.Streams.Messages;

namespace Orleans.Streams.Endpoints
{
    /// <summary>
    ///     Makes default stream operations available via its interface.
    /// </summary>
    public class StreamMessageSender<T> : IStreamMessageSender<T>, IBufferingStreamMessageSender
    {
        private readonly InternalStreamMessageSender _sender;
        private Queue<IStreamMessage> _messages;
        private List<Task> _awaitedSends;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="provider">Provider to use for stream.</param>
        /// <param name="guid">Identifier to use for the stream. Random Guid will be generated if default.</param>
        public StreamMessageSender(IStreamProvider provider, Guid guid = default(Guid))
        {
            _sender = new InternalStreamMessageSender(provider, guid);
            _messages = new Queue<IStreamMessage>();
            _awaitedSends = new List<Task>();
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="provider">Provider to use for stream.</param>
        /// <param name="targetStream">Identity of the stream to create.</param>
        public StreamMessageSender(IStreamProvider provider, StreamIdentity targetStream)
        {
            _sender = new InternalStreamMessageSender(provider, targetStream);
            _messages = new Queue<IStreamMessage>();
            _awaitedSends = new List<Task>();
        }


        public async Task StartTransaction(Guid transactionId)
        {
            await SendMessage(new TransactionMessage {State = TransactionState.Start, TransactionId = transactionId});
        }

        public async Task EndTransaction(Guid transactionId)
        {
            await SendMessage(new TransactionMessage {State = TransactionState.End, TransactionId = transactionId});
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

        public int FlushQueueSize { get; set; } = 512;

        public void EnqueueMessageBroadcast(IStreamMessage streamMessage)
        {
            EnqueueMessage(streamMessage);
        }

        public void EnqueueMessage(IStreamMessage streamMessage)
        {
            _messages.Enqueue(streamMessage);
            if (_messages.Count >= FlushQueueSize)
                FlushQueue();
        }

        private void FlushQueue()
        {
            if (_messages.Count > 0)
            {
                var combinedMessage = new CombinedMessage(_messages.ToList());
                _messages.Clear();
                _awaitedSends.Add(SendMessage(combinedMessage));
            }
            //while (_messages.Count > 0)
            //{
            //    var message = _messages.Dequeue();
            //    _awaitedSends.Add(SendMessage(message));
            //}
        }

        public async Task AwaitSendingComplete()
        {
            FlushQueue();
            await Task.WhenAll(_awaitedSends);
            _awaitedSends.Clear();
        }
    }
}