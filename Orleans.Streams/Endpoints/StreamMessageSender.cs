using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Orleans.Streams.Messages;

namespace Orleans.Streams.Endpoints
{
    public class StreamMessageSender : ITransactionalStreamProvider
    {
        public const string StreamNamespacePrefix = "StreamMessageSender";
        private readonly IAsyncStream<IStreamMessage> _messageStream;
        private bool _tearDownExecuted;
        private readonly Queue<IStreamMessage> _queue;
        private readonly StreamIdentity _streamIdentity;

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

        public async Task TearDown()
        {
            await _messageStream.OnCompletedAsync();
            _tearDownExecuted = true;
        }

        public Task<bool> IsTearedDown()
        {
            return Task.FromResult(_tearDownExecuted);
        }

        public Task<StreamIdentity> GetStreamIdentity()
        {
            return Task.FromResult(_streamIdentity);
        }

        public void AddToMessageQueue(IStreamMessage message)
        {
            _queue.Enqueue(message);
        }

        public async Task SendMessage(IStreamMessage message)
        {
            await _messageStream.OnNextAsync(message);
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