using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Orleans.Collections.Messages;
using Orleans.Streams.Endpoints;
using Orleans.Streams.Messages;

namespace Orleans.Streams.Linq.Nodes
{
    public abstract class StreamProcessorNodeGrain<TIn, TOut> : Grain, IStreamProcessorNodeGrain<TIn, TOut>
    {
        private const string StreamProviderNamespace = "CollectionStreamProvider";
        private SingleStreamTransactionReceiver _streamTransactionReceiver;
        protected StreamMessageDispatchReceiver StreamMessageDispatchReceiver;
        protected StreamMessageSenderFacade<TOut> StreamSender;

        public async Task SubscribeToStream(StreamIdentity inputStream)
        {
            await StreamMessageDispatchReceiver.Subscribe(inputStream);
        }

        public Task TransactionComplete(Guid transactionId)
        {
            return _streamTransactionReceiver.TransactionComplete(transactionId);
        }

        public async Task<IEnumerable<StreamIdentity>> GetOutputStreams()
        {
            return new List<StreamIdentity> { await StreamSender.GetStreamIdentity() };
        }

        public virtual async Task TearDown()
        {
            if (StreamMessageDispatchReceiver != null)
            {
                await StreamMessageDispatchReceiver.TearDown();
                StreamMessageDispatchReceiver = null;
            }

            if (StreamSender != null)
            {
                await StreamSender.TearDown();
                StreamSender = null;
            }
        }

        public async Task<bool> IsTearedDown()
        {
            var consumerTearDownState = (StreamMessageDispatchReceiver == null) || await StreamMessageDispatchReceiver.IsTearedDown();
            var providerTearDownState = (StreamSender == null) || await StreamSender.IsTearedDown();

            return consumerTearDownState && providerTearDownState;
        }

        public override Task OnActivateAsync()
        {
            base.OnActivateAsync();
            var streamMessageSender = new StreamMessageSender(GetStreamProvider(StreamProviderNamespace), this.GetPrimaryKey());
            StreamSender = new StreamMessageSenderFacade<TOut>(streamMessageSender);
            StreamMessageDispatchReceiver = new StreamMessageDispatchReceiver(GetStreamProvider(StreamProviderNamespace), TearDown);
            _streamTransactionReceiver = new SingleStreamTransactionReceiver(StreamMessageDispatchReceiver);
            RegisterMessages();
            return TaskDone.Done;
        }

        protected virtual void RegisterMessages()
        {
            StreamMessageDispatchReceiver.Register<TransactionMessage>(ProcessTransactionMessage);
        }

        protected async Task ProcessTransactionMessage(TransactionMessage transactionMessage)
        {
            // TODO: Make sure all items prior to sending the end message are processed when implementing methods not running on grain thread.
            if (transactionMessage.State == TransactionState.Start)
            {
                await StreamSender.StartTransaction(transactionMessage.TransactionId);
            }
            else if (transactionMessage.State == TransactionState.End)
            {
                await StreamSender.EndTransaction(transactionMessage.TransactionId);
            }
        }
    }
}