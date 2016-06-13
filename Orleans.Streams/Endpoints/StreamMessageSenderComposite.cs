using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Orleans.Streams.Messages;

namespace Orleans.Streams.Endpoints
{
    /// <summary>
    /// Sends messages via multiple children output streams.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class StreamMessageSenderComposite<T> : IStreamMessageSenderComposite<T>
    {
        /// <summary>
        /// Senders that are used for sending. Each of them is mapped to one output stream.
        /// </summary>
        protected List<StreamMessageSender<T>> Senders;

        private readonly IStreamProvider _provider;
        private int _nextSenderIndex;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="provider">Provider to use for the stream.</param>
        /// <param name="numberOfOutputStreams">Number of output streams to create.</param>
        public StreamMessageSenderComposite(IStreamProvider provider, int numberOfOutputStreams = 0)
        {
            _provider = provider;
            if (numberOfOutputStreams < 0)
            {
                throw new ArgumentException(nameof(numberOfOutputStreams));
            }

            Senders = Enumerable.Range(0, numberOfOutputStreams).Select(i => new StreamMessageSender<T>(provider)).ToList();
            _nextSenderIndex = 0;
        }

        /// <summary>
        /// Sets the number of senders and tears down the senders if there are too many.
        /// </summary>
        /// <param name="numberOfOutputStreams"></param>
        /// <returns></returns>
        public async Task SetNumberOfSenders(int numberOfOutputStreams)
        {
            if(numberOfOutputStreams < 0)
                throw new ArgumentOutOfRangeException(nameof(numberOfOutputStreams));
            while (numberOfOutputStreams < Senders.Count)
            {
                await Senders[0].TearDown();
            }

            int sendersToAdd = numberOfOutputStreams - Senders.Count;

            Senders.AddRange(Enumerable.Range(0, sendersToAdd).Select(i => new StreamMessageSender<T>(_provider)).ToList());
        }


         /// <summary>
        ///     Sends a generic message via all output channels.
        /// </summary>
        /// <param name="message">Message to send.</param>
        /// <returns></returns>
        public async Task SendMessage(IStreamMessage message)
        {
            await Senders.First().SendMessage(message);
        }

        /// <summary>
        ///     Sends a message to all outputs.
        /// </summary>
        /// <param name="message">Message to send.</param>
        /// <returns></returns>
        public async Task SendMessageBroadcast(IStreamMessage message)
        {
            await Task.WhenAll(Senders.Select(s => s.SendMessage(message)));
        }

        /// <summary>
        ///     Starts a new transaction.
        /// </summary>
        /// <param name="transactionId">Identifier.</param>
        /// <returns></returns>
        public async Task StartTransaction(Guid transactionId)
        {
            await Task.WhenAll(Senders.Select(s => s.StartTransaction(transactionId)));
        }

        /// <summary>
        ///     Ends a transaction.
        /// </summary>
        /// <param name="transactionId">Identifier.</param>
        /// <returns></returns>
        public async Task EndTransaction(Guid transactionId)
        {
            await Task.WhenAll(Senders.Select(s => s.EndTransaction(transactionId)));
        }

        /// <summary>
        /// Get identities of the provided output streams.
        /// </summary>
        /// <returns></returns>
        public async Task<IList<StreamIdentity>> GetOutputStreams()
        {
            var listOfLists = await Task.WhenAll(Senders.Select(s => s.GetOutputStreams()));
            return listOfLists.SelectMany(streams => streams).ToList();
        }

        /// <summary>
        /// Tear down all output streams.
        /// </summary>
        /// <returns></returns>
        public async Task TearDown()
        {
            await Task.WhenAll(Senders.Select(s => s.TearDown()));
        }


        /// <summary>
        /// Checks if this all output streams are teared down.
        /// </summary>
        /// <returns></returns>
        public async Task<bool> IsTearedDown()
        {
            var sendersTearedDown = await Task.WhenAll(Senders.Select(s => s.IsTearedDown()));

            return sendersTearedDown.All(tearedDown => tearedDown);
        }

        public int FlushQueueSize { get; set; } = 512;

        /// <summary>
        ///     Enqueues a message that is sent to all outputs once the FlushQueueSize is reached.
        /// </summary>
        /// <param name="message">Message to send.</param>
        /// <returns></returns>
        public void EnqueueMessageBroadcast(IStreamMessage streamMessage)
        {
            foreach (var sender in Senders)
            {
                sender.EnqueueMessageBroadcast(streamMessage);
            }
        }

        /// <summary>
        /// Enqeues a message to one output once the FlushQueueSize is reached.
        /// </summary>
        /// <param name="streamMessage">Message to send.</param>
        public void EnqueueMessage(IStreamMessage streamMessage)
        {
            Senders[_nextSenderIndex].EnqueueMessage(streamMessage);
            _nextSenderIndex = (_nextSenderIndex + 1)%Senders.Count;
        }

        /// <summary>
        /// Awaits all sent messages.
        /// </summary>
        /// <returns></returns>
        public async Task AwaitSendingComplete()
        {
            await Task.WhenAll(Senders.Select(s => s.AwaitSendingComplete()));
        }
    }
}