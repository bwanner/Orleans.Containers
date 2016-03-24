using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace Orleans.Collections
{
    /// <summary>
    ///     Stores elements in a container context and manages their references.
    /// </summary>
    public class ContainerElementList<T> : ICollectionOperations<T>, IBatchItemAdder<T>
    {
        private readonly Guid _containerId;
        private readonly IElementExecutor<T> _executorGrainReference;
        private readonly IElementExecutor<T> _executorReference;
        protected List<T> Collection;

        public T this[ContainerElementReference<T> reference]
        {
            get
            {
                if (!reference.ContainerId.Equals(_containerId))
                {
                    throw new ArgumentException();
                }

                return Collection[reference.Offset];
            }
        }

        public IReadOnlyList<T> Elements => Collection;

        public ContainerElementList(Guid containerId, IElementExecutor<T> executorReference, IElementExecutor<T> executorGrainReference)
        {
            _containerId = containerId;
            _executorReference = executorReference;
            _executorGrainReference = executorGrainReference;
            Collection = new List<T>();
        }

        public Task<IReadOnlyCollection<ContainerElementReference<T>>> AddRange(IEnumerable<T> items)
        {
            var oldCount = Collection.Count;
            foreach (var item in items)
            {
                Collection.Add(item);
            }

            IReadOnlyCollection<ContainerElementReference<T>> newReferences =
                Enumerable.Range(oldCount, Collection.Count - oldCount).Select(i => GetReferenceForItem(i)).ToList();
            return Task.FromResult(newReferences);
        }

        public Task Clear()
        {
            Collection.Clear();
            return TaskDone.Done;
        }

        public Task<bool> Contains(T item)
        {
            return Task.FromResult(Collection.Contains(item));
        }

        public Task<int> Count()
        {
            return Task.FromResult(Collection.Count);
        }

        public Task<bool> Remove(ContainerElementReference<T> reference)
        {
            if (!reference.ContainerId.Equals(_containerId))
            {
                throw new ArgumentException();
            }

            if (Collection.Count < reference.Offset)
            {
                return Task.FromResult(false);
            }

            Collection.RemoveAt(reference.Offset);

            return Task.FromResult(true);
        }


        protected ContainerElementReference<T> GetReferenceForItem(int offset, bool itemExists = true)
        {
            return new ContainerElementReference<T>(_containerId, offset, _executorReference,
                _executorGrainReference, itemExists);
        }
    }
}