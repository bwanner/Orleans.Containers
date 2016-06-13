﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Orleans.Streams.Messages;

namespace Orleans.Streams.Endpoints
{
    /// <summary>
    ///     Internal stream message sender implementation to abstract form Orleans Streams.
    /// </summary>
    internal class InternalStreamMessageSender
    {
        public const string StreamNamespacePrefix = "StreamMessageSender";
        private readonly IAsyncStream<IStreamMessage> _messageStream;
        private readonly StreamIdentity _streamIdentity;
        private bool _tearDownExecuted;

        public InternalStreamMessageSender(IStreamProvider provider, Guid guid = default(Guid))
        {
            guid = guid == default(Guid) ? Guid.NewGuid() : guid;
            _streamIdentity = new StreamIdentity(guid, StreamNamespacePrefix);
            _messageStream = provider.GetStream<IStreamMessage>(_streamIdentity.Guid, _streamIdentity.Namespace);
            _tearDownExecuted = false;
        }

        public InternalStreamMessageSender(IStreamProvider provider, StreamIdentity targetStream)
        {
            _streamIdentity = targetStream;
            _messageStream = provider.GetStream<IStreamMessage>(_streamIdentity.Guid, _streamIdentity.Namespace);
            _tearDownExecuted = false;
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

        public async Task SendMessage(IStreamMessage message)
        {
            await _messageStream.OnNextAsync(message);
        }
    }
}