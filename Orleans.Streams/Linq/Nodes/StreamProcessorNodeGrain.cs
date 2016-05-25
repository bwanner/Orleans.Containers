using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Orleans.Streams.Endpoints;
using Orleans.Streams.Messages;

namespace Orleans.Streams.Linq.Nodes
{
    public abstract class StreamProcessorNodeGrain<TIn, TOut> : Grain, IStreamProcessorNodeGrain<TIn, TOut>
    {
        protected const string StreamProviderNamespace = "CollectionStreamProvider"; // TODO replace with config value
        protected TransactionalStreamConsumer StreamConsumer;
        protected StreamMessageSender<TOut> StreamSender;

        public async Task SubscribeToStreams(IEnumerable<StreamIdentity> inputStream)
        {
            await StreamConsumer.SetInput(inputStream.ToList());
        }

        public Task TransactionComplete(Guid transactionId)
        {
            return StreamConsumer.TransactionComplete(transactionId);
        }

        public Task<IList<StreamIdentity>> GetOutputStreams()
        {
            return StreamSender.GetOutputStreams();
        }

        public async Task<bool> IsTearedDown()
        {
            var consumerTearDownState = (StreamConsumer == null) || await StreamConsumer.IsTearedDown();
            var providerTearDownState = (StreamSender == null) || await StreamSender.IsTearedDown();

            return consumerTearDownState && providerTearDownState;
        }

        public async Task<IList<SiloLocationStreamIdentity>> GetOutputStreamsWithSourceLocation()
        {
            var outputStreams = await GetOutputStreams();
            var siloIdentity = Enumerable.Repeat(RuntimeIdentity, outputStreams.Count);

            return
                outputStreams.Zip(siloIdentity,
                    (streamIdentity, siloId) => new SiloLocationStreamIdentity(streamIdentity.Guid, streamIdentity.Namespace, siloId)).ToList();
        }

        public virtual async Task TearDown()
        {
            if (StreamConsumer != null)
            {
                await StreamConsumer.TearDown();
                StreamConsumer = null;
            }

            if (StreamSender != null)
            {
                await StreamSender.TearDown();
                StreamSender = null;
            }
        }

        public override Task OnActivateAsync()
        {
            base.OnActivateAsync();
            StreamConsumer = new TransactionalStreamConsumer(GetStreamProvider(StreamProviderNamespace), TearDown);
            StreamSender = new StreamMessageSender<TOut>(GetStreamProvider(StreamProviderNamespace), this.GetPrimaryKey());
            RegisterMessages();
            return TaskDone.Done;
        }

        protected virtual void RegisterMessages()
        {
            StreamConsumer.MessageDispatcher.Register<FlushMessage>(ProcessFlushMessage);
            StreamConsumer.MessageDispatcher.Register<TransactionMessage>(ProcessTransactionMessage);
        }

        protected async Task ProcessTransactionMessage(TransactionMessage transactionMessage)
        {
            if (transactionMessage.State == TransactionState.End)
                await StreamSender.AwaitSendingComplete();
            // TODO: Make sure all items prior to sending the end message are processed when implementing methods not running on grain thread.
            await StreamSender.SendMessage(transactionMessage);
        }

        private async Task ProcessFlushMessage(FlushMessage message)
        {
            await StreamSender.SendMessage(message);
        }
    }
}