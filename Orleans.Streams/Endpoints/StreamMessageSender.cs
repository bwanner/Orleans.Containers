using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Orleans.Streams.Messages;

namespace Orleans.Streams.Endpoints
{
    public class StreamMessageSender
    {
        public const string StreamNamespacePrefix = "StreamMessageSender";
        private readonly IAsyncStream<IStreamMessage> _messageStream;
        private readonly Queue<IStreamMessage> _queue;
        private readonly StreamIdentity _streamIdentity;
        private bool _tearDownExecuted;

        public StreamMessageSender(IStreamProvider provider, Guid guid = default(Guid))
        {
            guid = guid == default(Guid) ? Guid.NewGuid() : guid;
            _streamIdentity = new StreamIdentity(StreamNamespacePrefix, guid);
            _messageStream = provider.GetStream<IStreamMessage>(_streamIdentity.StreamIdentifier.Item1, _streamIdentity.StreamIdentifier.Item2);
            _tearDownExecuted = false;
            _queue = new Queue<IStreamMessage>();
        }

        public StreamMessageSender(IStreamProvider provider, StreamIdentity targetStream)
        {
            _streamIdentity = targetStream;
            _messageStream = provider.GetStream<IStreamMessage>(_streamIdentity.StreamIdentifier.Item1, _streamIdentity.StreamIdentifier.Item2);
            _tearDownExecuted = false;
            _queue = new Queue<IStreamMessage>();
        }

        public Task<StreamIdentity> GetStreamIdentity()
        {
            return Task.FromResult(_streamIdentity);
        }

        public Task<bool> IsTearedDown()
        {
            return Task.FromResult(_tearDownExecuted);
        }

        public async Task TearDown()
        {
            await _messageStream.OnCompletedAsync();
            _tearDownExecuted = true;
        }

        public void AddToMessageQueue(IStreamMessage message)
        {
            _queue.Enqueue(message);
        }

        public async Task SendMessage(IStreamMessage message)
        {
            await _messageStream.OnNextAsync(message);
        }

        public async Task StartTransaction(Guid transactionId)
        {
            await SendMessage(new TransactionMessage {State = TransactionState.Start, TransactionId = transactionId});
        }

        public async Task EndTransaction(Guid transactionId)
        {
            await SendMessage(new TransactionMessage {State = TransactionState.End, TransactionId = transactionId});
        }

        public async Task SendMessagesFromQueue()
        {
            while (_queue.Count > 0)
            {
                var message = _queue.Dequeue();
                await SendMessage(message);
            }
        }
    }
}