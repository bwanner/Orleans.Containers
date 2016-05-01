using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Orleans.Streams.Messages;

namespace Orleans.Streams.Endpoints
{
    /// <summary>
    /// Sends messages via multiple children output.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class StreamMessageSenderComposite<T> : IStreamMessageSender<T>
    {
        private List<StreamMessageSender<T>> _senders;

        public StreamMessageSenderComposite(IStreamProvider provider, int numberOfChildren = 1)
        {
            if (numberOfChildren <= 0)
            {
                throw new ArgumentException(nameof(numberOfChildren));
            }

            _senders = Enumerable.Range(0, numberOfChildren).Select(i => new StreamMessageSender<T>(provider)).ToList();
        }

        public async Task SendItems(IEnumerable<T> items)
        {
            var split = _senders.SplitEquallyBetweenSenders(items);
            await Task.WhenAll(split.Select(tuple => tuple.Item1.SendItems(tuple.Item2)));
        }

        public async Task StartTransaction(Guid transactionId)
        {
            await Task.WhenAll(_senders.Select(s => s.StartTransaction(transactionId)));
        }

        public async Task EndTransaction(Guid transactionId)
        {
            await Task.WhenAll(_senders.Select(s => s.EndTransaction(transactionId)));
        }

        public async Task SendAddItems(IEnumerable<T> items)
        {
            var split = _senders.SplitEquallyBetweenSenders(items);
            await Task.WhenAll(split.Select(tuple => tuple.Item1.SendAddItems(tuple.Item2)));
        }

        public async Task SendUpdateItems(IEnumerable<T> items)
        {
            var split = _senders.SplitEquallyBetweenSenders(items);
            await Task.WhenAll(split.Select(tuple => tuple.Item1.SendUpdateItems(tuple.Item2)));
        }

        public async Task SendRemoveItems(IEnumerable<T> items)
        {
            var split = _senders.SplitEquallyBetweenSenders(items);
            await Task.WhenAll(split.Select(tuple => tuple.Item1.SendRemoveItems(tuple.Item2)));
        }

        public void EnqueueAddItems(IEnumerable<T> items)
        {
            var split = _senders.SplitEquallyBetweenSenders(items);
            foreach (var tuple in split)
            {
                tuple.Item1.EnqueueAddItems(tuple.Item2);
            }
        }

        public void EnqueueUpdateItems(IEnumerable<T> items)
        {
            var split = _senders.SplitEquallyBetweenSenders(items);
            foreach (var tuple in split)
            {
                tuple.Item1.EnqueueUpdateItems(tuple.Item2);
            }
        }

        public void EnqueueRemoveItems(IEnumerable<T> items)
        {
            var split = _senders.SplitEquallyBetweenSenders(items);
            foreach (var tuple in split)
            {
                tuple.Item1.EnqueueRemoveItems(tuple.Item2);
            }
        }

        public void EnqueueMessage(IStreamMessage message)
        {
            _senders.First().EnqueueMessage(message);
        }

        public async Task FlushQueue()
        {
            await Task.WhenAll(_senders.Select(s => s.FlushQueue()));
        }

        public async Task<IList<StreamIdentity>> GetOutputStreams()
        {
            var listOfLists = await Task.WhenAll(_senders.Select(s => s.GetOutputStreams()));
            return listOfLists.SelectMany(streams => streams).ToList();
        }

        public async Task TearDown()
        {
            await Task.WhenAll(_senders.Select(s => s.TearDown()));
        }

        public async Task<bool> IsTearedDown()
        {
            var sendersTearedDown = await Task.WhenAll(_senders.Select(s => s.IsTearedDown()));

            return sendersTearedDown.All(tearedDown => tearedDown);
        }
    }
}