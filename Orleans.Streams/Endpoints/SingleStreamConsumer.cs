using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Orleans.Streams.Endpoints
{
    /// <summary>
    /// Consumes items of a single stream.
    /// </summary>
    /// <typeparam name="TIn">Type of items to consume.</typeparam>
    public class SingleStreamConsumer<TIn> : ITransactionalStreamConsumer<TIn>
    {
        private readonly Dictionary<int, TaskCompletionSource<Task>> _awaitedTransactions;
        private readonly IStreamProvider _streamProvider;
        private readonly Func<Task> _tearDownFunc;
        private StreamSubscriptionHandle<IEnumerable<TIn>> _itemBatchStreamHandle;
        private StreamSubscriptionHandle<StreamTransaction> _transactionStreamHandle;
        private readonly Func<StreamTransaction, Task> _streamTransactionReceivedFunc;
        private readonly Func<IEnumerable<TIn>, Task> _streamItemBatchReceivedFunc;
        private bool _tearDownExecuted;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="streamProvider">Stream provider to be used.</param>
        /// <param name="streamItemBatchReceivedFunc">Asynchronous function to be executed when an item is received.</param>
        /// <param name="streamTransactionReceivedFunc">Asynchronous function to be executed when a transaction message is received.</param>
        /// <param name="tearDownFunc">Asynchronous function to be executed on tear down.</param>
        public SingleStreamConsumer(IStreamProvider streamProvider, Func<IEnumerable<TIn>, Task> streamItemBatchReceivedFunc = null, Func<StreamTransaction, Task> streamTransactionReceivedFunc = null, Func<Task> tearDownFunc = null)
        {
            _streamProvider = streamProvider;
            _tearDownFunc = tearDownFunc;
            _awaitedTransactions = new Dictionary<int, TaskCompletionSource<Task>>();
            _streamTransactionReceivedFunc = streamTransactionReceivedFunc;
            _streamItemBatchReceivedFunc = streamItemBatchReceivedFunc;
        }

        public async Task SetInput(TransactionalStreamIdentity<TIn> inputStream)
        {
            _tearDownExecuted = false;
            var streamTransaction = _streamProvider.GetStream<StreamTransaction>(inputStream.TransactionStreamIdentifier.Item1, inputStream.TransactionStreamIdentifier.Item2);

            _transactionStreamHandle =
                await streamTransaction.SubscribeAsync((item, token) => TransactionMessageArrived(item), async () => await TearDown());


            var streamItem = _streamProvider.GetStream<IEnumerable<TIn>>(inputStream.ItemBatchStreamIdentifier.Item1, inputStream.ItemBatchStreamIdentifier.Item2);

            if (_streamItemBatchReceivedFunc != null)
            {
                _itemBatchStreamHandle = await streamItem.SubscribeAsync((item, token) => _streamItemBatchReceivedFunc(item));
            }
        }

        public virtual async Task TearDown()
        {
            if (!_tearDownExecuted)
            {
                _tearDownExecuted = true;
                if (_transactionStreamHandle != null)
                {
                    await _transactionStreamHandle.UnsubscribeAsync();
                }
                if (_itemBatchStreamHandle != null)
                {
                    await _itemBatchStreamHandle.UnsubscribeAsync();
                }
                _transactionStreamHandle = null;
                _itemBatchStreamHandle = null;
                if (_tearDownFunc != null)
                {
                    await _tearDownFunc();
                }
            }
        }

        public Task<bool> IsTearedDown()
        {
            return Task.FromResult(_tearDownExecuted);
        }

        /// <summary>
        /// Returns if transaction is completed.
        /// </summary>
        /// <param name="transactionId">Transaction identifier.</param>
        /// <returns></returns>
        public async Task TransactionComplete(int transactionId)
        {
            if (!_awaitedTransactions.ContainsKey(transactionId))
            {
                _awaitedTransactions[transactionId] = new TaskCompletionSource<Task>();
            }

            await _awaitedTransactions[transactionId].Task;
        }

        private async Task TransactionMessageArrived(StreamTransaction transaction)
        {
            if (transaction.State == TransactionState.Start)
            {
                if (!_awaitedTransactions.ContainsKey(transaction.TransactionId))
                {
                    _awaitedTransactions[transaction.TransactionId] = new TaskCompletionSource<Task>();
                }
            }

            else if (transaction.State == TransactionState.End)
            {
                _awaitedTransactions[transaction.TransactionId].SetResult(TaskDone.Done);
            }

            if (_streamTransactionReceivedFunc != null)
            {
                await _streamTransactionReceivedFunc(transaction);
            }
        }
    }
}