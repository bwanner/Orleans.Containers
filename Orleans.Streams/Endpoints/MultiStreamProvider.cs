using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Orleans.Runtime;

namespace Orleans.Streams.Endpoints
{
    /// <summary>
    /// Provides multiple transactional streams.
    /// </summary>
    /// <typeparam name="T">Type of items to stream.</typeparam>
    public class MultiStreamProvider<T> : ITransactionalStreamProviderAggregate<T>
    {
        private readonly List<SingleStreamTransactionSender<T>> _providers;

        private bool _tearDownExecuted;
        private List<StreamMessageSender> _senders;

        public MultiStreamProvider(IStreamProvider provider, int numberOutputStreams)
        {
            _senders = Enumerable.Range(0, numberOutputStreams).Select(i => new StreamMessageSender(provider)).ToList();
            _providers = _senders.Select(s => new SingleStreamTransactionSender<T>(s)).ToList();
            _tearDownExecuted = false;
        }

        public async Task<IList<StreamIdentity>> GetStreamIdentities()
        {
            var result = await Task.WhenAll(_senders.Select(s => s.GetStreamIdentity()));

            return result.ToList();
        }

        public Task<bool> IsTearedDown()
        {
            return Task.FromResult(_tearDownExecuted);
        }

        public async Task TearDown()
        {
            await Task.WhenAll(_senders.Select(s => s.TearDown()));
            _tearDownExecuted = true;
        }

        /// <summary>
        /// Send items to output streams as one transaction. Items are evenly distributed across providers.
        /// </summary>
        /// <param name="data">Items to send.</param>
        /// <returns>Transaction identifier.</returns>
        public async Task<Guid> SendItems(ICollection<T> data)
        {
            var transactionId = Guid.NewGuid();

            var itemsPerProvider = (int) Math.Ceiling(data.Count/(double) _providers.Count);
            var chunks = data.BatchIEnumerable(itemsPerProvider);
            await Task.WhenAll(_providers.Select(p => p.StartTransaction(transactionId)));
            var tasks =
                _providers.Zip(chunks, (p, c) => new Tuple<SingleStreamTransactionSender<T>, IList<T>>(p, c))
                    .Select(t => t.Item1.SendItems(t.Item2, false));
            var taskResults = await Task.WhenAll(tasks);
            await Task.WhenAll(_providers.Select(p => p.EndTransaction(transactionId)));

            return transactionId;
        }
    }
}