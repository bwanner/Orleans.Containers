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
        protected StreamMessageSender<ContainerElement<T>> OutputProducer;

        public virtual Task<IReadOnlyCollection<ContainerElementReference<T>>> AddRange(IEnumerable<T> items)
        {
            return Task.FromResult(Elements.AddRange(items));
        }

        protected StreamMessageSender<ContainerElement<T>> SetupSenderStream(StreamIdentity streamIdentity)
        {
            var transactionalSender = new StreamMessageSender<ContainerElement<T>>(GetStreamProvider(StreamProviderName), streamIdentity);

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

            await OutputProducer.FlushQueue();
        }

        public Task<IList<object>> ExecuteAsync(Func<T, Task<object>> func)
        {
            return ExecuteAsync((x, state) => func(x), null);
        }

        public async Task<IList<object>> ExecuteAsync(Func<T, object, Task<object>> func, object state)
        {
            var results = Elements.Elements.Select(item => func(item, state)).ToList();
            var resultSet = await Task.WhenAll(results);
            await OutputProducer.FlushQueue();
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
            await OutputProducer.FlushQueue();
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

            await OutputProducer.FlushQueue();
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

            await OutputProducer.FlushQueue();
            return result;
        }

        public async Task<IList<object>> ExecuteSync(Func<T, object, object> func, object state)
        {
            IList<object> results = Elements.Elements.Select(item => func(item, state)).ToList();
            await OutputProducer.FlushQueue();
            return results;
        }

        public Task<object> ExecuteSync(Func<T, object> func, ContainerElementReference<T> reference = null)
        {
            return ExecuteSync((x, state) => func(x), null, reference);
        }

        public async Task SubscribeToStreams(IEnumerable<StreamIdentity> inputStream)
        {
            await Task.WhenAll(inputStream.Select(s => StreamMessageDispatchReceiver.Subscribe(s)));
        }

        public Task TransactionComplete(Guid transactionId)
        {
            return _streamTransactionReceiver.TransactionComplete(transactionId);
        }

        public async Task<IList<StreamIdentity>> GetOutputStreams()
        {
            return await OutputProducer.GetOutputStreams();
        }

        public async Task<bool> IsTearedDown()
        {
            var tearDownStates = await Task.WhenAll(StreamMessageDispatchReceiver.IsTearedDown(), OutputProducer.IsTearedDown());

            return tearDownStates[0] && tearDownStates[1];
        }

        public async Task TearDown()
        {
            await OutputProducer.TearDown();
        }

        public override async Task OnActivateAsync()
        {
            OutputProducer = new StreamMessageSender<ContainerElement<T>>(GetStreamProvider(StreamProviderName), this.GetPrimaryKey());
            StreamMessageDispatchReceiver = new StreamMessageDispatchReceiver(GetStreamProvider(StreamProviderName), GetLogger(), TearDown);
            _streamTransactionReceiver = new SingleStreamTransactionReceiver(StreamMessageDispatchReceiver);
            StreamMessageDispatchReceiver.Register<ItemAddMessage<T>>(ProcessItemMessage);
            Elements = new ContainerElementList<T>(this.GetPrimaryKey(), this, this.AsReference<IContainerNodeGrain<T>>());
            await base.OnActivateAsync();
        }

        protected virtual async Task ProcessItemMessage(ItemAddMessage<T> message)
        {
            await AddRange(message.Items);
        }
    }
}