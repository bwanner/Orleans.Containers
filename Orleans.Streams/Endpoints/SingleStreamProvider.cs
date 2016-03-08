using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Orleans.Streams.Endpoints
{
    /// <summary>
    ///     Provides a single transactional stream.
    /// </summary>
    /// <typeparam name="T">Data type transmitted via the stream</typeparam>
    public class SingleStreamProvider<T> : ITransactionalStreamProvider<T>
    {
        public const string StreamNamespacePrefix = "SingleStreamProvider";
        private readonly IAsyncStream<IEnumerable<T>> _itemBatchStream;
        private readonly TransactionalStreamIdentity<T> _streamIdentity;
        private readonly IAsyncStream<StreamTransaction> _transactionStream;
        private int _lastTransactionId = -1;
        private bool _tearDownExecuted;

        public SingleStreamProvider(IStreamProvider provider, Guid guid = default(Guid))
        {
            guid = guid == default(Guid) ? Guid.NewGuid() : guid;
            _streamIdentity = new TransactionalStreamIdentity<T>(StreamNamespacePrefix, guid);
            _itemBatchStream = provider.GetStream<IEnumerable<T>>(_streamIdentity.ItemBatchStreamIdentifier.Item1,
                _streamIdentity.ItemBatchStreamIdentifier.Item2);
            _transactionStream = provider.GetStream<StreamTransaction>(_streamIdentity.TransactionStreamIdentifier.Item1,
                _streamIdentity.TransactionStreamIdentifier.Item2);
            _tearDownExecuted = false;
        }

        public Task<TransactionalStreamIdentity<T>> GetStreamIdentity()
        {
            return Task.FromResult(_streamIdentity);
        }

        public async Task TearDown()
        {
            _tearDownExecuted = true;
            await _itemBatchStream.OnCompletedAsync();
            await _transactionStream.OnCompletedAsync();
        }

        public Task<bool> IsTearedDown()
        {
            return Task.FromResult(_tearDownExecuted);
        }

        public async Task<int> SendItems(IEnumerable<T> items, bool useTransaction = true, int? transactionId = null)
        {
            var curTransactionId = transactionId ?? ++_lastTransactionId;
            if (useTransaction)
            {
                await StartTransaction(curTransactionId);
            }
            await Task.WhenAll(_itemBatchStream.OnNextAsync(items));
            if (useTransaction)
            {
                await EndTransaction(curTransactionId);
            }

            return curTransactionId;
        }

        public Task<int> SendItem(T item, bool useTransaction = true, int? transactionId = null)
        {
            return SendItems(new List<T>(1) {item}, useTransaction, transactionId);
        }

        public async Task StartTransaction(int transactionId)
        {
            await _transactionStream.OnNextAsync(new StreamTransaction {State = TransactionState.Start, TransactionId = transactionId});
        }

        public async Task EndTransaction(int transactionId)
        {
            await _transactionStream.OnNextAsync(new StreamTransaction {State = TransactionState.End, TransactionId = transactionId});
        }
    }
}