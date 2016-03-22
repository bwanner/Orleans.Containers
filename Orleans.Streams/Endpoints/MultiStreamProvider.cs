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
        private readonly List<SingleStreamProvider<T>> _providers;
        private int _lastTransactionId = -1;

        private bool _tearDownExecuted;

        public MultiStreamProvider(IStreamProvider provider, int numberOutputStreams)
        {
            _providers = Enumerable.Range(0, numberOutputStreams).Select(i => new SingleStreamProvider<T>(provider)).ToList();
            _tearDownExecuted = false;
        }

        public async Task<IList<StreamIdentity<T>>> GetStreamIdentities()
        {
            var result = await Task.WhenAll(_providers.Select(p => p.GetStreamIdentity()));

            return result.ToList();
        }

        public Task<bool> IsTearedDown()
        {
            return Task.FromResult(_tearDownExecuted);
        }

        public async Task TearDown()
        {
            await Task.WhenAll(_providers.Select(p => p.TearDown()));
            _tearDownExecuted = true;
        }

        /// <summary>
        /// Send items to output streams. Items are evenly distributed across providers.
        /// </summary>
        /// <param name="data">Items to send.</param>
        /// <returns>Transaction identifier.</returns>
        public async Task<int> SendItems(ICollection<T> data)
        {
            var transactionId = ++_lastTransactionId;

            var itemsPerProvider = (int) Math.Ceiling(data.Count/(double) _providers.Count);
            var chunks = data.BatchIEnumerable(itemsPerProvider);
            var tasks =
                _providers.Zip(chunks, (p, c) => new Tuple<SingleStreamProvider<T>, IList<T>>(p, c))
                    .Select(t => t.Item1.SendItems(t.Item2, true, transactionId));
            var taskResults = await Task.WhenAll(tasks);

            return transactionId;
        }
    }
}