using System.Collections.Generic;
using System.Threading.Tasks;
using Orleans.Streams.Endpoints;
using Orleans.Streams.Messages;

namespace Orleans.Streams.Linq.Nodes
{
    public abstract class StreamProcessorNodeGrain<TIn, TOut> : Grain, IStreamProcessorNodeGrain<TIn, TOut>
    {
        private const string StreamProviderNamespace = "CollectionStreamProvider";
        private SingleStreamTransactionManager _streamTransactionManager;
        protected StreamMessageDispatcher StreamMessageDispatcher;
        protected SingleStreamProvider<TOut> StreamProvider;

        public async Task SetInput(StreamIdentity<TIn> inputStream)
        {
            await StreamMessageDispatcher.Subscribe(inputStream.StreamIdentifier);
        }

        public Task TransactionComplete(int transactionId)
        {
            return _streamTransactionManager.TransactionComplete(transactionId);
        }

        public async Task<StreamIdentity<TOut>> GetStreamIdentity()
        {
            return await StreamProvider.GetStreamIdentity();
        }

        public virtual async Task TearDown()
        {
            if (StreamMessageDispatcher != null)
            {
                await StreamMessageDispatcher.TearDown();
                _streamTransactionManager = null;
            }

            if (StreamProvider != null)
            {
                await StreamProvider.TearDown();
                StreamProvider = null;
            }
        }

        public async Task<bool> IsTearedDown()
        {
            var consumerTearDownState = (_streamTransactionManager == null) || await StreamMessageDispatcher.IsTearedDown();
            var providerTearDownState = (StreamProvider == null) || await StreamProvider.IsTearedDown();

            return consumerTearDownState && providerTearDownState;
        }

        public override Task OnActivateAsync()
        {
            base.OnActivateAsync();
            StreamProvider = new SingleStreamProvider<TOut>(GetStreamProvider(StreamProviderNamespace), this.GetPrimaryKey());
            StreamMessageDispatcher = new StreamMessageDispatcher(GetStreamProvider(StreamProviderNamespace), TearDown);
            _streamTransactionManager = new SingleStreamTransactionManager(StreamMessageDispatcher);
            StreamMessageDispatcher.Register<TransactionMessage>(ProcessTransactionMessage);
            StreamMessageDispatcher.Register<ItemMessage<TIn>>(ProcessItemMessage);
            return TaskDone.Done;
        }

        protected abstract Task ProcessItemMessage(ItemMessage<TIn> itemMessage);

        protected async Task ProcessTransactionMessage(TransactionMessage transactionMessage)
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