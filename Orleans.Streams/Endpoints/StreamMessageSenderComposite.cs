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
    public class StreamMessageSenderComposite<T> : IStreamMessageSender<T>
    {
        /// <summary>
        /// Senders that are used for sending. Each of them is mapped to one output stream.
        /// </summary>
        protected List<StreamMessageSender<T>> Senders;

        private IStreamProvider _provider;

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
        /// Send items via this strem.
        /// </summary>
        /// <param name="items">Items to send.</param>
        /// <returns></returns>
        public async Task SendItems(IEnumerable<T> items)
        {
            var split = Senders.SplitEquallyBetweenSenders(items);
            await Task.WhenAll(split.Select(tuple => tuple.Item1.SendItems(tuple.Item2)));
        }

        /// <summary>
        /// Start a transaction.
        /// </summary>
        /// <param name="transactionId">Transaction identifier.</param>
        /// <returns></returns>
        public async Task StartTransaction(Guid transactionId)
        {
            await Task.WhenAll(Senders.Select(s => s.StartTransaction(transactionId)));
        }

        /// <summary>
        /// End a transaction.
        /// </summary>
        /// <param name="transactionId">Transaction identifier.</param>
        /// <returns></returns>
        public async Task EndTransaction(Guid transactionId)
        {
            await Task.WhenAll(Senders.Select(s => s.EndTransaction(transactionId)));
        }

        /// <summary>
        /// Send a AddItemMessage for all items.
        /// </summary>
        /// <param name="items">Items to send.</param>
        /// <returns></returns>
        public async Task SendAddItems(IEnumerable<T> items)
        {
            var split = Senders.SplitEquallyBetweenSenders(items);
            await Task.WhenAll(split.Select(tuple => tuple.Item1.SendAddItems(tuple.Item2)));
        }

        /// <summary>
        /// Send a UpdateItemMessage for all items.
        /// </summary>
        /// <param name="items">Items to send.</param>
        /// <returns></returns>
        public async Task SendUpdateItems(IEnumerable<T> items)
        {
            var split = Senders.SplitEquallyBetweenSenders(items);
            await Task.WhenAll(split.Select(tuple => tuple.Item1.SendUpdateItems(tuple.Item2)));
        }

        /// <summary>
        /// Send a RemoveItemMessage for all items.
        /// </summary>
        /// <param name="items">Items to send.</param>
        /// <returns></returns>
        public async Task SendRemoveItems(IEnumerable<T> items)
        {
            var split = Senders.SplitEquallyBetweenSenders(items);
            await Task.WhenAll(split.Select(tuple => tuple.Item1.SendRemoveItems(tuple.Item2)));
        }

        /// <summary>
        ///     Sends a generic message.
        /// </summary>
        /// <param name="message">Message to send.</param>
        /// <returns></returns>
        public async Task SendMessage(IStreamMessage message)
        {
            await Senders.First().SendMessage(message);
        }

        /// <summary>
        /// Enqueue a AddItemMessage for all items.
        /// </summary>
        /// <param name="items">Items to send.</param>
        /// <returns></returns>
        public void EnqueueAddItems(IEnumerable<T> items)
        {
            var split = Senders.SplitEquallyBetweenSenders(items);
            foreach (var tuple in split)
            {
                tuple.Item1.EnqueueAddItems(tuple.Item2);
            }
        }

        public void EnqueueUpdateItems(IEnumerable<T> items)
        {
            var split = Senders.SplitEquallyBetweenSenders(items);
            foreach (var tuple in split)
            {
                tuple.Item1.EnqueueUpdateItems(tuple.Item2);
            }
        }

        /// <summary>
        /// Enqueue a RemoveItemMessage for all items.
        /// </summary>
        /// <param name="items">Items to send.</param>
        /// <returns></returns>
        public void EnqueueRemoveItems(IEnumerable<T> items)
        {
            var split = Senders.SplitEquallyBetweenSenders(items);
            foreach (var tuple in split)
            {
                tuple.Item1.EnqueueRemoveItems(tuple.Item2);
            }
        }

        /// <summary>
        /// Enqeue a generic message.
        /// </summary>
        /// <returns></returns>
        public void EnqueueMessage(IStreamMessage message)
        {
            Senders.First().EnqueueMessage(message);
        }

        /// <summary>
        /// Send all messages and empty the queue.
        /// </summary>
        /// <returns></returns>
        public async Task FlushQueue()
        {
            await Task.WhenAll(Senders.Select(s => s.FlushQueue()));
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
    }
}