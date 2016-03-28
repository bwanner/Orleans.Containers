using System.Collections.Generic;
using System.Threading.Tasks;
using Orleans.Streams.Endpoints;
using Orleans.Streams.Messages;

namespace Orleans.Streams.Linq.Nodes
{
    public abstract class StreamProcessorNodeGrain<TIn, TOut> : Grain, IStreamProcessorNodeGrain<TIn, TOut>
    {
        private const string StreamProviderNamespace = "CollectionStreamProvider";
        private SingleStreamTransactionReceiver _streamTransactionReceiver;
        protected StreamMessageDispatchReceiver StreamMessageDispatchReceiver;
        protected SingleStreamTransactionSender<TOut> StreamTransactionSender;
        protected StreamMessageSender StreamMessageSender;

        public async Task SetInput(StreamIdentity inputStream)
        {
            await StreamMessageDispatchReceiver.Subscribe(inputStream);
        }

        public Task TransactionComplete(int transactionId)
        {
            return _streamTransactionReceiver.TransactionComplete(transactionId);
        }

        public async Task<StreamIdentity> GetStreamIdentity()
        {
            return await StreamMessageSender.GetStreamIdentity();
        }

        public virtual async Task TearDown()
        {
            if (StreamMessageDispatchReceiver != null)
            {
                await StreamMessageDispatchReceiver.TearDown();
                StreamMessageDispatchReceiver = null;
            }

            if (StreamMessageSender != null)
            {
                await StreamMessageSender.TearDown();
                StreamMessageSender = null;
            }
        }

        public async Task<bool> IsTearedDown()
        {
            var consumerTearDownState = (StreamMessageDispatchReceiver == null) || await StreamMessageDispatchReceiver.IsTearedDown();
            var providerTearDownState = (StreamMessageSender == null) || await StreamMessageSender.IsTearedDown();

            return consumerTearDownState && providerTearDownState;
        }

        public override Task OnActivateAsync()
        {
            base.OnActivateAsync();
            StreamMessageSender = new StreamMessageSender(GetStreamProvider(StreamProviderNamespace), this.GetPrimaryKey());
            StreamTransactionSender = new SingleStreamTransactionSender<TOut>(StreamMessageSender);
            StreamMessageDispatchReceiver = new StreamMessageDispatchReceiver(GetStreamProvider(StreamProviderNamespace), TearDown);
            _streamTransactionReceiver = new SingleStreamTransactionReceiver(StreamMessageDispatchReceiver);
            RegisterMessages();
            return TaskDone.Done;
        }

        protected virtual void RegisterMessages()
        {
            StreamMessageDispatchReceiver.Register<TransactionMessage>(ProcessTransactionMessage);
            StreamMessageDispatchReceiver.Register<ItemMessage<TIn>>(ProcessItemMessage);
        }

        protected abstract Task ProcessItemMessage(ItemMessage<TIn> itemMessage);

        protected async Task ProcessTransactionMessage(TransactionMessage transactionMessage)
        {
            // TODO: Make sure all items prior to sending the end message are processed when implementing methods not running on grain thread.
            if (transactionMessage.State == TransactionState.Start)
            {
                await StreamTransactionSender.StartTransaction(transactionMessage.TransactionId);
            }
            else if (transactionMessage.State == TransactionState.End)
            {
                await StreamTransactionSender.EndTransaction(transactionMessage.TransactionId);
            }
        }
    }
}