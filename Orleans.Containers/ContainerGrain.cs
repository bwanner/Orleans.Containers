﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Orleans.Streams;

namespace Orleans.Collections
{
    public class ContainerGrain<T> : Grain, IGrainWithIntegerKey, IContainerGrain<T>
    {
        private const int NumberContainersStart = 1;
        private List<IContainerNodeGrain<T>> _containers;
        private bool _tearDownExecuted;

        public Task<ICollection<IBatchItemAdder<T>>> GetItemAdders()
        {
            ICollection<IBatchItemAdder<T>> readers = _containers.Cast<IBatchItemAdder<T>>().ToList();

            return Task.FromResult(readers);
        }

        public async Task Clear()
        {
            await Task.WhenAll(_containers.Select(async x => await x.Clear()).ToList());
            _containers.Clear();
        }

        public async Task<bool> Contains(T item)
        {
            var resultTask = Task.WhenAll(_containers.Select(async c => await c.Contains(item)));
            var results = await resultTask;

            return results.Contains(true);
        }

        public async Task<int> Count()
        {
            var resultTask = await Task.WhenAll(_containers.Select(async container => await container.Count()));
            return resultTask.Sum();
        }

        public async Task<bool> Remove(ContainerElementReference<T> reference)
        {
            var container = _containers.First(c => c.GetPrimaryKey().Equals(reference.ContainerId));
            if (container != null)
            {
                return await container.Remove(reference);
            }

            return false;
        }

        public async Task<Guid> EnumerateToSubscribers(int batchSize)
        {
            var transactionId = Guid.NewGuid();
            await Task.WhenAll(_containers.Select(c => c.EnumerateToSubscribers(transactionId)));

            return transactionId;
        }

        public async Task<Guid> EnumerateToStream(params StreamIdentity[] streamIdentities)
        {
            var transactionId = Guid.NewGuid();
            var assignedStreams = streamIdentities.Repeat().Take(_containers.Count);

            await Task.WhenAll(assignedStreams.Zip(_containers, (identity, container) => container.EnumerateToStream(identity, transactionId)));

            return transactionId;
        }

        public async Task SetNumberOfNodes(int numContainer)
        {
            var containersToAdd = numContainer - _containers.Count;
            if (containersToAdd < 0)
            {
                throw new NotImplementedException("Merging containers is not implemented yet.");
            }


            var initTasks = new List<Task>();
            for (var i = 0; i < containersToAdd; i++)
            {
                var containerNode = CreateContainerGrain();
                //initTasks.Add(await containerNode.Clear());
                _containers.Add(containerNode);
            }

            await Task.WhenAll(initTasks);
        }

        public async Task ExecuteAsync(Func<T, Task> func, ContainerElementReference<T> reference = null)
        {
            if (reference != null)
            {
                var container = _containers.First(c => c.GetPrimaryKey().Equals(reference.ContainerId));
                await container.ExecuteAsync(func, reference);
            }
            else
            {
                await Task.WhenAll(_containers.Select(c => c.ExecuteAsync(func)));
            }
        }

        public async Task ExecuteAsync(Func<T, object, Task> func, object state, ContainerElementReference<T> reference = null)
        {
            if (reference != null)
            {
                var container = _containers.First(c => c.GetPrimaryKey().Equals(reference.ContainerId));
                await container.ExecuteAsync(func, state, reference);
            }
            else
            {
                await Task.WhenAll(_containers.Select(c => c.ExecuteAsync(func, state)));
            }
        }

        public async Task<IList<object>> ExecuteAsync(Func<T, Task<object>> func)
        {
            var result = await Task.WhenAll(_containers.Select(c => c.ExecuteAsync(func)));
            return new List<object>(result);
        }

        public async Task<IList<object>> ExecuteAsync(Func<T, object, Task<object>> func, object state)
        {
            var result = await Task.WhenAll(_containers.Select(c => c.ExecuteAsync(func, state)));
            return new List<object>(result);
        }

        public async Task<object> ExecuteAsync(Func<T, Task<object>> func, ContainerElementReference<T> reference)
        {
            var container = _containers.First(c => c.GetPrimaryKey().Equals(reference.ContainerId));
            return await container.ExecuteAsync(func, reference);
        }

        public async Task<object> ExecuteAsync(Func<T, object, Task<object>> func, object state, ContainerElementReference<T> reference)
        {
            var container = _containers.First(c => c.GetPrimaryKey().Equals(reference.ContainerId));
            return await container.ExecuteAsync(func, state, reference);
        }

        public async Task ExecuteSync(Action<T> action, ContainerElementReference<T> reference = null)
        {
            if (reference != null)
            {
                var container = _containers.First(c => c.GetPrimaryKey().Equals(reference.ContainerId));
                await container.ExecuteSync(action, reference);
            }
            else
            {
                await Task.WhenAll(_containers.Select(c => c.ExecuteSync(action, null)));
            }
        }

        public async Task ExecuteSync(Action<T, object> action, object state, ContainerElementReference<T> reference = null)
        {
            if (reference != null)
            {
                var container = _containers.First(c => c.GetPrimaryKey().Equals(reference.ContainerId));
                await container.ExecuteSync(action, state, reference);
            }
            else
            {
                await Task.WhenAll(_containers.Select(c => c.ExecuteSync(action, state)));
            }
        }

        public async Task<IList<object>> ExecuteSync(Func<T, object, object> func, object state)
        {
            var result = Task.WhenAll(_containers.Select(c => c.ExecuteSync(func, state)));
            return await result;
        }

        public async Task<IList<object>> ExecuteSync(Func<T, object> func)
        {
            var result = Task.WhenAll(_containers.Select(c => c.ExecuteSync(func)));
            return await result;
        }

        public async Task<object> ExecuteSync(Func<T, object, object> func, object state, ContainerElementReference<T> reference)
        {
            var container = _containers.First(c => c.GetPrimaryKey().Equals(reference.ContainerId));
            return await container.ExecuteSync(func, state, reference);
        }

        public async Task<object> ExecuteSync(Func<T, object> func, ContainerElementReference<T> reference)
        {
            var container = _containers.First(c => c.GetPrimaryKey().Equals(reference.ContainerId));
            return await container.ExecuteSync(func, reference);
        }

        public async Task SetInput(IList<StreamIdentity> streamIdentities)
        {
            _tearDownExecuted = false;
            if (streamIdentities.Count() != _containers.Count)
            {
                throw new ArgumentException();
            }

            await
                Task.WhenAll(_containers.Zip(streamIdentities,
                    (grain, identity) => new Tuple<IContainerNodeGrain<T>, StreamIdentity>(grain, identity))
                    .Select(t => t.Item1.SubscribeToStreams(t.Item2.SingleValueToList())));
        }

        public async Task TransactionComplete(Guid transactionId)
        {
            await Task.WhenAll(_containers.Select(c => c.TransactionComplete(transactionId)));
        }

        public async Task<IList<StreamIdentity>> GetOutputStreams()
        {
            var streamTasks = await Task.WhenAll(_containers.Select(c => c.GetOutputStreams()));

            var resultingStreams = streamTasks.SelectMany(s => s).ToList();
            return resultingStreams;
        }

        public Task<bool> IsTearedDown()
        {
            return Task.FromResult(_tearDownExecuted);
        }

        public Task<IList<SiloLocationStreamIdentity>> GetOutputStreamsWithSourceLocation()
        {
            throw new NotImplementedException();
        }

        public Task SetInput(IList<SiloLocationStreamIdentity> streamIdentities)
        {
            throw new NotImplementedException();
        }

        public async Task TearDown()
        {
            await Task.WhenAll(_containers.Select(c => c.TearDown()));
        }

        public override async Task OnActivateAsync()
        {
            _containers = new List<IContainerNodeGrain<T>>();
            await SetNumberOfNodes(NumberContainersStart);
            await base.OnActivateAsync();
        }

        internal virtual IContainerNodeGrain<T> CreateContainerGrain()
        {
            return GrainFactory.GetGrain<IContainerNodeGrain<T>>(Guid.NewGuid());
        }
    }
}