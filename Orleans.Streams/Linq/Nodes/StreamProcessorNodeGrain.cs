using System.Collections.Generic;
using System.Threading.Tasks;
using Orleans.Streams.Endpoints;
using Orleans.Streams.Messages;

namespace Orleans.Streams.Linq.Nodes
{
    public abstract class StreamProcessorNodeGrain<TIn, TOut> : Grain, IStreamProcessorNodeGrain<TIn, TOut>
    {
        private const string StreamProviderNamespace = "CollectionStreamProvider";
        private SingleStreamConsumer<TIn> _streamConsumer;
        protected SingleStreamProvider<TOut> StreamProvider;

        public async Task SetInput(StreamIdentity<TIn> inputStream)
        {
            await _streamConsumer.SetInput(inputStream);
        }

        public Task TransactionComplete(int transactionId)
        {
            return _streamConsumer.TransactionComplete(transactionId);
        }

        public async Task<StreamIdentity<TOut>> GetStreamIdentity()
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
            _streamConsumer = new SingleStreamConsumer<TIn>(GetStreamProvider(StreamProviderNamespace), this,
                TearDown);
            return TaskDone.Done;
        }

        public abstract Task Visit(ItemMessage<TIn> message);

        public async Task Visit(TransactionMessage transactionMessage)
        {
            // TODO: Make sure all items prior to sending the end message are processed when implementing methods not running on grain thread.
            if (transactionMessage.State == TransactionState.Start)
            {
                await StreamProvider.StartTransaction(transactionMessage.TransactionId);
            }
            else if (transactionMessage.State == TransactionState.End)
            {
                await StreamProvider.EndTransaction(transactionMessage.TransactionId);
            }
        }
    }
}