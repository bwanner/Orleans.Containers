using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Orleans.Collections.Messages;
using Orleans.Streams.Endpoints;
using Orleans.Streams.Messages;

namespace Orleans.Streams.Linq.Nodes
{
    public abstract class StreamProcessorNodeGrain<TIn, TOut> : Grain, IStreamProcessorNodeGrain<TIn, TOut>
    {
        protected const string StreamProviderNamespace = "CollectionStreamProvider";
        protected TransactionalStreamConsumer StreamConsumer;
        protected IStreamMessageSender<TOut> StreamSender;

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

        public async Task<bool> IsTearedDown()
        {
            var consumerTearDownState = (StreamConsumer == null) || await StreamConsumer.IsTearedDown();
            var providerTearDownState = (StreamSender == null) || await StreamSender.IsTearedDown();

            return consumerTearDownState && providerTearDownState;
        }

        public override Task OnActivateAsync()
        {
            base.OnActivateAsync();
            StreamConsumer = new TransactionalStreamConsumer(GetStreamProvider(StreamProviderNamespace));
            RegisterMessages();
            return TaskDone.Done;
        }

        protected virtual void RegisterMessages()
        {
            StreamConsumer.MessageDispatcher.Register<FlushMessage>(ProcessFlushMessage);
        }

        private async Task ProcessFlushMessage(FlushMessage message)
        {
            await StreamSender.FlushQueue();
            await StreamSender.SendMessage(message);
        }
    }
}