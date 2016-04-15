using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Orleans.Collections.Endpoints;
using Orleans.Collections.Messages;
using Orleans.Collections.Utilities;
using Orleans.Streams;
using Orleans.Streams.Endpoints;
using Orleans.Streams.Messages;

namespace Orleans.Collections
{
    /// <summary>
    ///     Implementation of a containerNode grain.
    /// </summary>
    public class ContainerNodeGrain<T> : Grain, IContainerNodeGrain<T>
    {
        private const string StreamProviderName = "CollectionStreamProvider";
        protected StreamMessageDispatchReceiver StreamMessageDispatchReceiver;
        private SingleStreamTransactionReceiver _streamTransactionReceiver;
        protected ContainerElementList<T> Elements;
        protected StreamMessageSenderFacade<ContainerElement<T>> OutputProducer;

        public virtual Task<IReadOnlyCollection<ContainerElementReference<T>>> AddRange(IEnumerable<T> items)
        {
            return Task.FromResult(Elements.AddRange(items));
        }

        protected StreamMessageSenderFacade<ContainerElement<T>> SetupSenderStream(StreamIdentity streamIdentity)
        {
            var sender = new StreamMessageSender(GetStreamProvider(StreamProviderName), streamIdentity);
            var transactionalSender = new StreamMessageSenderFacade<ContainerElement<T>>(sender);

            return transactionalSender;
        }

        public async Task<Guid> EnumerateToStream(StreamIdentity streamIdentity, Guid transactionId)
        {
            var transactionalSender = SetupSenderStream(streamIdentity);
            await transactionalSender.StartTransaction(transactionId);
            var elements = Elements.ToList();
            await transactionalSender.SendAddItems(elements);
            await transactionalSender.EndTransaction(transactionId);
            await transactionalSender.TearDown();
            return transactionId;
        }

        public virtual Task Clear()
        {
            Elements.Clear();
            return TaskDone.Done;
        }

        public Task<bool> Contains(T item)
        {
            return Elements.Contains(item);
        }

        public Task<int> Count()
        {
            return Elements.Count();
        }

        public virtual Task<bool> Remove(ContainerElementReference<T> reference)
        {
            return Elements.Remove(reference);
        }

        public async Task<Guid> EnumerateToSubscribers(Guid? transactionId = null)
        {
            var tId = TransactionGenerator.GenerateTransactionId(transactionId);
            await OutputProducer.StartTransaction(tId);
            await OutputProducer.SendAddItems(Elements);
            await OutputProducer.EndTransaction(tId);

            return tId;
        }

        public Task ExecuteAsync(Func<T, Task> func, ContainerElementReference<T> reference = null)
        {
            return ExecuteAsync((x, state) => func(x), null, reference);
        }

        public async Task ExecuteAsync(Func<T, object, Task> func, object state, ContainerElementReference<T> reference = null)
        {
            if (reference != null)
            {
                var curItem = Elements.GetElement(reference);
                await func(curItem, state);
            }

            else
            {
                foreach (var item in Elements.Elements)
                {
                    await func(item, state);
                }
            }

            await StreamMessageSender.SendMessagesFromQueue();
        }

        public Task<IList<object>> ExecuteAsync(Func<T, Task<object>> func)
        {
            return ExecuteAsync((x, state) => func(x), null);
        }

        public async Task<IList<object>> ExecuteAsync(Func<T, object, Task<object>> func, object state)
        {
            var results = Elements.Elements.Select(item => func(item, state)).ToList();
            var resultSet = await Task.WhenAll(results);
            await StreamMessageSender.SendMessagesFromQueue();
            return new List<object>(resultSet);
        }

        public Task<object> ExecuteAsync(Func<T, Task<object>> func, ContainerElementReference<T> reference)
        {
            return ExecuteAsync((x, state) => func(x), null, reference);
        }

        public async Task<object> ExecuteAsync(Func<T, object, Task<object>> func, object state, ContainerElementReference<T> reference)
        {
            var curItem = Elements.GetElement(reference);
            var result = await func(curItem, state);
            await StreamMessageSender.SendMessagesFromQueue();
            return result;
        }

        public Task ExecuteSync(Action<T> action, ContainerElementReference<T> reference = null)
        {
            return ExecuteSync((x, state) => action(x), null, reference);
        }

        public async Task ExecuteSync(Action<T, object> action, object state, ContainerElementReference<T> reference = null)
        {
            if (reference != null)
            {
                var curItem = Elements.GetElement(reference);
                action(curItem, state);
            }
            else
            {
                foreach (var item in Elements.Elements)
                {
                    action(item, state);
                }
            }

            await StreamMessageSender.SendMessagesFromQueue();
        }

        public Task<IList<object>> ExecuteSync(Func<T, object> func)
        {
            return ExecuteSync((x, state) => func(x), null);
        }

        public async Task<object> ExecuteSync(Func<T, object, object> func, object state, ContainerElementReference<T> reference)
        {
            if (!this.GetPrimaryKey().Equals(reference.ContainerId))
            {
                throw new InvalidOperationException();
            }
            var curItem = Elements.GetElement(reference);
            var result = func(curItem, state);

            await StreamMessageSender.SendMessagesFromQueue();
            return result;
        }

        public async Task<IList<object>> ExecuteSync(Func<T, object, object> func, object state)
        {
            IList<object> results = Elements.Elements.Select(item => func(item, state)).ToList();
            await StreamMessageSender.SendMessagesFromQueue();
            return results;
        }

        public Task<object> ExecuteSync(Func<T, object> func, ContainerElementReference<T> reference = null)
        {
            return ExecuteSync((x, state) => func(x), null, reference);
        }

        public async Task SetInput(StreamIdentity inputStream)
        {
            await StreamMessageDispatchReceiver.Subscribe(inputStream);
        }

        public Task TransactionComplete(Guid transactionId)
        {
            return _streamTransactionReceiver.TransactionComplete(transactionId);
        }

        public async Task<StreamIdentity> GetStreamIdentity()
        {
            return await StreamMessageSender.GetStreamIdentity();
        }

        public async Task<bool> IsTearedDown()
        {
            var tearDownStates = await Task.WhenAll(StreamMessageDispatchReceiver.IsTearedDown(), StreamMessageSender.IsTearedDown());

            return tearDownStates[0] && tearDownStates[1];
        }

        public async Task TearDown()
        {
            await StreamMessageSender.TearDown();
        }

        public override async Task OnActivateAsync()
        {
            StreamMessageSender = new StreamMessageSender(GetStreamProvider(StreamProviderName), this.GetPrimaryKey());
            OutputProducer = new StreamMessageSenderFacade<ContainerElement<T>>(StreamMessageSender);
            StreamMessageDispatchReceiver = new StreamMessageDispatchReceiver(GetStreamProvider(StreamProviderName), TearDown);
            _streamTransactionReceiver = new SingleStreamTransactionReceiver(StreamMessageDispatchReceiver);
            StreamMessageDispatchReceiver.Register<ItemAddMessage<T>>(ProcessItemMessage);
            Elements = new ContainerElementList<T>(this.GetPrimaryKey(), this, this.AsReference<IContainerNodeGrain<T>>());
            await base.OnActivateAsync();
        }

        public StreamMessageSender StreamMessageSender { get; set; }

        protected virtual async Task ProcessItemMessage(ItemAddMessage<T> message)
        {
            await AddRange(message.Items);
        }
    }
}