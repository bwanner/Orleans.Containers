using System.Collections.Generic;
using System.Threading.Tasks;
using Orleans.Streams.Endpoints;

namespace Orleans.Streams.Linq.Nodes
{
    public abstract class StreamProcessorNodeGrain<TIn, TOut> : Grain, IStreamProcessorNodeGrain<TIn, TOut>
    {
        private const string StreamProviderNamespace = "CollectionStreamProvider";
        private SingleStreamConsumer<TIn> _streamConsumer;
        protected SingleStreamProvider<TOut> StreamProvider;

        public async Task SetInput(TransactionalStreamIdentity<TIn> inputStream)
        {
            await _streamConsumer.SetInput(inputStream);
        }

        public Task TransactionComplete(int transactionId)
        {
            return _streamConsumer.TransactionComplete(transactionId);
        }

        public async Task<TransactionalStreamIdentity<TOut>> GetStreamIdentity()
        {
            return await StreamProvider.GetStreamIdentity();
        }

        public virtual async Task TearDown()
        {
            if (_streamConsumer != null)
            {
                await _streamConsumer.TearDown();
                _streamConsumer = null;
            }

            if (StreamProvider != null)
            {
                await StreamProvider.TearDown();
                StreamProvider = null;
            }
        }

        public async Task<bool> IsTearedDown()
        {
            var consumerTearDownState = (_streamConsumer == null) || await _streamConsumer.IsTearedDown();
            var providerTearDownState = (StreamProvider == null) || await StreamProvider.IsTearedDown();

            return consumerTearDownState && providerTearDownState;
        }

        public override Task OnActivateAsync()
        {
            base.OnActivateAsync();
            StreamProvider = new SingleStreamProvider<TOut>(GetStreamProvider(StreamProviderNamespace), this.GetPrimaryKey());
            _streamConsumer = new SingleStreamConsumer<TIn>(GetStreamProvider(StreamProviderNamespace), ItemArrived, TransactionMessageArrived,
                TearDown);
            return TaskDone.Done;
        }

        protected virtual async Task TransactionMessageArrived(StreamTransaction transaction)
        {
            // TODO: Make sure all items prior to sending the end message are processed when implementing methods not running on grain thread.
            if (transaction.State == TransactionState.Start)
            {
                await StreamProvider.StartTransaction(transaction.TransactionId);
            }
            else if (transaction.State == TransactionState.End)
            {
                await StreamProvider.EndTransaction(transaction.TransactionId);
            }
        }

        protected abstract Task ItemArrived(IEnumerable<TIn> items);
    }
}