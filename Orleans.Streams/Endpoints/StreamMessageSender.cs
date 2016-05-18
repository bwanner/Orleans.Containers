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
        private readonly Queue<IStreamMessage> _messages;
        private readonly List<Task> _awaitedSends;

        /// <summary>
        ///     Constructor.
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
        ///     Constructor.
        /// </summary>
        /// <param name="provider">Provider to use for stream.</param>
        /// <param name="targetStream">Identity of the stream to create.</param>
        public StreamMessageSender(IStreamProvider provider, StreamIdentity targetStream)
        {
            _sender = new InternalStreamMessageSender(provider, targetStream);
            _messages = new Queue<IStreamMessage>();
            _awaitedSends = new List<Task>();
        }

        /// <summary>
        ///     Awaits all sent messages.
        /// </summary>
        /// <returns></returns>
        public async Task AwaitSendingComplete()
        {
            FlushQueue();
            await Task.WhenAll(_awaitedSends);
            _awaitedSends.Clear();
        }

        /// <summary>
        ///     Enqeues a message to one output once the FlushQueueSize is reached.
        /// </summary>
        /// <param name="streamMessage">Message to send.</param>
        public void EnqueueMessage(IStreamMessage streamMessage)
        {
            _messages.Enqueue(streamMessage);
            if (_messages.Count >= FlushQueueSize)
                FlushQueue();
        }

        /// <summary>
        ///     Enqueues a message that is sent to all outputs once the FlushQueueSize is reached.
        /// </summary>
        /// <param name="streamMessage">Message to send.</param>
        /// <returns></returns>
        public void EnqueueMessageBroadcast(IStreamMessage streamMessage)
        {
            EnqueueMessage(streamMessage);
        }

        /// <summary>
        ///     Minimum size that the messages are sent.
        /// </summary>
        public int FlushQueueSize { get; set; } = 512;

        /// <summary>
        ///     Ends a transaction.
        /// </summary>
        /// <param name="transactionId">Identifier.</param>
        /// <returns></returns>
        public async Task EndTransaction(Guid transactionId)
        {
            await SendMessage(new TransactionMessage {State = TransactionState.End, TransactionId = transactionId});
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

        /// <summary>
        ///     Sends a message to all outputs.
        /// </summary>
        /// <param name="message">Message to send.</param>
        /// <returns></returns>
        public async Task SendMessageBroadcast(IStreamMessage message)
        {
            await SendMessage(message);
        }


        /// <summary>
        ///     Starts a new transaction.
        /// </summary>
        /// <param name="transactionId">Identifier.</param>
        /// <returns></returns>
        public async Task StartTransaction(Guid transactionId)
        {
            await SendMessage(new TransactionMessage {State = TransactionState.Start, TransactionId = transactionId});
        }

        /// <summary>
        ///     Get identities of the provided output streams.
        /// </summary>
        /// <returns></returns>
        public async Task<IList<StreamIdentity>> GetOutputStreams()
        {
            return (await _sender.GetStreamIdentity()).SingleValueToList();
        }

        /// <summary>
        ///     Checks if this output stream is teared down.
        /// </summary>
        /// <returns></returns>
        public Task<bool> IsTearedDown()
        {
            return _sender.IsTearedDown();
        }

        /// <summary>
        ///     Tears down the output stream.
        /// </summary>
        /// <returns></returns>
        public Task TearDown()
        {
            return _sender.TearDown();
        }

        private void FlushQueue()
        {
            if (_messages.Count > 0)
            {
                var combinedMessage = new CombinedMessage(_messages.ToList());
                _messages.Clear();
                _awaitedSends.Add(SendMessage(combinedMessage));
            }
        }
    }
}