using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Orleans.Collections
{
    /// <summary>
    ///     Stores elements in a container context and manages their references.
    /// </summary>
    public class ContainerElementList<T> : ICollectionOperations<T>, IEnumerable<ContainerElement<T>>
    {
        private readonly Guid _containerId;
        private readonly IElementExecutor<T> _executorGrainReference;
        private readonly IElementExecutor<T> _executorReference;
        protected List<T> Collection;

        public ContainerElement<T> this[ContainerElementAddress<T> address]
        {
            get
            {
                if (!address.ContainerId.Equals(_containerId))
                {
                    throw new ArgumentException();
                }

                return CreateContainerElement(address.Offset);
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

        public virtual Task Clear()
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

        public virtual Task<bool> Remove(ContainerElementReference<T> reference)
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

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IEnumerator<ContainerElement<T>> GetEnumerator()
        {
            return
                Enumerable.Range(0, Collection.Count)
                    .Select(CreateContainerElement)
                    .GetEnumerator();
        }

        public T GetElement(ContainerElementAddress<T> address)
        {
            if (!address.ContainerId.Equals(_containerId))
            {
                throw new ArgumentException();
            }

            return Collection[address.Offset];
        }

        public void SetElement(ContainerElementAddress<T> address, T value)
        {
            if (!address.ContainerId.Equals(_containerId))
            {
                throw new ArgumentException();
            }

            Collection[address.Offset] = value;
        }

        public virtual IReadOnlyCollection<ContainerElementReference<T>> AddRange(IEnumerable<T> items)
        {
            var oldCount = Collection.Count;
            foreach (var item in items)
            {
                Collection.Add(item);
            }

            IReadOnlyCollection<ContainerElementReference<T>> newReferences =
                Enumerable.Range(oldCount, Collection.Count - oldCount).Select(i => GetReferenceForItem(i)).ToList();
            return newReferences;
        }

        public ContainerElementReference<T> Remove(T item)
        {
            var index = Collection.IndexOf(item);
            if (index == -1)
            {
                throw new ArgumentException(nameof(item));
            }

            Collection.Remove(item);
            return GetReferenceForItem(index, false);
        }

        protected ContainerElementReference<T> GetReferenceForItem(int offset, bool itemExists = true)
        {
            return new ContainerElementReference<T>(_containerId, offset, _executorReference,
                _executorGrainReference, itemExists);
        }

        private ContainerElement<T> CreateContainerElement(int offset)
        {
            return new ContainerElement<T>(GetReferenceForItem(offset), Collection[offset]);
        }
    }
}