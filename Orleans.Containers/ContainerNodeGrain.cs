using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
        protected ContainerElementList<T> List;
        protected SingleStreamProvider<ContainerElement<T>> StreamProvider;
        private SingleStreamTransactionManager _streamTransactionManager;
        private StreamMessageDispatcher _streamMessageDispatcher;
        private const string StreamProviderName = "CollectionStreamProvider";

        public virtual Task<IReadOnlyCollection<ContainerElementReference<T>>> AddRange(IEnumerable<T> items)
        {
            return List.AddRange(items);
        }

        public async Task EnumerateItems(ICollection<IBatchItemAdder<T>> adders)
        {
            await adders.BatchAdd(List.Elements);
        }

        public Task Clear()
        {
            List.Clear();
            return TaskDone.Done;
        }

        public Task<bool> Contains(T item)
        {
            return List.Contains(item);
        }

        public Task<int> Count()
        {
            return List.Count();
        }

        public virtual Task<bool> Remove(ContainerElementReference<T> reference)
        {
            return List.Remove(reference);
        }

        public async Task<int> EnumerateToStream(int? transactionId = null)
        {
            var hostedItems = List.Elements.Select((value, index) => new ContainerElement<T>(GetReferenceForItem(index), value)).ToList();

            return await StreamProvider.SendItems(hostedItems, true, transactionId);
        }

        public override async Task OnActivateAsync()
        {
            List = new ContainerElementList<T>(this.GetPrimaryKey(), this, this.AsReference<IContainerNodeGrain<T>>());
            StreamProvider = new SingleStreamProvider<ContainerElement<T>>(GetStreamProvider(StreamProviderName), this.GetPrimaryKey());
            _streamMessageDispatcher = new StreamMessageDispatcher(GetStreamProvider(StreamProviderName), TearDown);
            _streamTransactionManager = new SingleStreamTransactionManager(_streamMessageDispatcher);
            _streamMessageDispatcher.Register<ItemMessage<T>>(ProcessItemMessage);
            await base.OnActivateAsync();
        }

        protected ContainerElementReference<T> GetReferenceForItem(int offset, bool exists = true)
        {
            return new ContainerElementReference<T>(this.GetPrimaryKey(), offset, this,
                this.AsReference<IContainerNodeGrain<T>>(), exists);
        }

        public async Task<StreamIdentity<ContainerElement<T>>> GetStreamIdentity()
        {
            return await StreamProvider.GetStreamIdentity();
        }

        public async Task SetInput(StreamIdentity<T> inputStream)
        {
            await _streamMessageDispatcher.Subscribe(inputStream.StreamIdentifier);
        }

        public Task TransactionComplete(int transactionId)
        {
            return _streamTransactionManager.TransactionComplete(transactionId);
        }

        public async Task TearDown()
        {
            await StreamProvider.TearDown();
        }

        public async Task<bool> IsTearedDown()
        {
            var tearDownStates = await Task.WhenAll(_streamMessageDispatcher.IsTearedDown(), StreamProvider.IsTearedDown());

            return tearDownStates[0] && tearDownStates[1];
        }

        public Task ExecuteSync(Action<T> action, ContainerElementReference<T> reference = null)
        {
            return ExecuteSync((x, state) => action(x), null, reference);
        }

        public Task ExecuteSync(Action<T, object> action, object state, ContainerElementReference<T> reference = null)
        {
            if (reference != null)
            {
                var curItem = List[reference];
                action(curItem, state);
            }
            else
            {
                foreach (var item in List.Elements)
                {
                    action(item, state);
                }
            }

            return TaskDone.Done;
        }

        public Task<IList<object>> ExecuteSync(Func<T, object> func)
        {
            return ExecuteSync((x, state) => func(x), null);
        }

        public Task<object> ExecuteSync(Func<T, object, object> func, object state, ContainerElementReference<T> reference)
        {
            if (!this.GetPrimaryKey().Equals(reference.ContainerId))
            {
                throw new InvalidOperationException();
            }
            var curItem = List[reference];
            var result = func(curItem, state);
            return Task.FromResult(result);
        }

        public Task<IList<object>> ExecuteSync(Func<T, object, object> func, object state)
        {
            IList<object> results = List.Elements.Select(item => func(item, state)).ToList();
            return Task.FromResult(results);
        }

        public Task<object> ExecuteSync(Func<T, object> func, ContainerElementReference<T> reference = null)
        {
            return ExecuteSync((x, state) => func(x), null, reference);
        }

        public Task ExecuteAsync(Func<T, Task> func, ContainerElementReference<T> reference = null)
        {
            return ExecuteAsync((x, state) => func(x), null, reference);
        }

        public async Task ExecuteAsync(Func<T, object, Task> func, object state, ContainerElementReference<T> reference = null)
        {
            if (reference != null)
            {
                var curItem = List[reference];
                await func(curItem, state);
            }

            else
            {
                foreach (var item in List.Elements)
                {
                    await func(item, state);
                }
            }
        }

        public Task<IList<object>> ExecuteAsync(Func<T, Task<object>> func)
        {
            return ExecuteAsync((x, state) => func(x), null);
        }

        public async Task<IList<object>> ExecuteAsync(Func<T, object, Task<object>> func, object state)
        {
            var results = List.Elements.Select(item => func(item, state)).ToList();
            var resultSet = await Task.WhenAll(results);
            return new List<object>(resultSet);
        }

        public Task<object> ExecuteAsync(Func<T, Task<object>> func, ContainerElementReference<T> reference)
        {
            return ExecuteAsync((x, state) => func(x), null, reference);
        }

        public async Task<object> ExecuteAsync(Func<T, object, Task<object>> func, object state, ContainerElementReference<T> reference)
        {
            var curItem = List[reference];
            var result = await func(curItem, state);
            return result;
        }

        public async Task ProcessItemMessage(ItemMessage<T> message)
        {
            await AddRange(message.Items);
        }
    }
}