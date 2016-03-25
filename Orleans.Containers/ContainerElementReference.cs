using System;

namespace Orleans.Collections
{
    [Serializable]
    public class ContainerElementReference<T>
    {
        private readonly IElementExecutor<T> _executorGrainReference;

        [NonSerialized] private readonly IElementExecutor<T> _executorReference;

        public Guid ContainerId { get; }
        public int Offset { get; }

        public IElementExecutor<T> Executor => _executorReference ?? _executorGrainReference;
        public bool Exists { get; }

        public ContainerElementReference(Guid containerId, int offset, IElementExecutor<T> executorReference,
            IElementExecutor<T> executorGrainReference, bool exists = true)
        {
            ContainerId = containerId;
            Offset = offset;
            _executorReference = executorReference;
            _executorGrainReference = executorGrainReference;
            Exists = exists;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((ContainerElementReference<T>) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (ContainerId.GetHashCode()*397) ^ Offset;
            }
        }

        protected bool Equals(ContainerElementReference<T> other)
        {
            return ContainerId.Equals(other.ContainerId) && Offset == other.Offset;
        }
    }
}