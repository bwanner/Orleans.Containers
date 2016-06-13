using System;

namespace Orleans.Collections
{
    [Serializable]
    public class ContainerElementReference<T> : ContainerElementAddress<T>
    {
        private readonly IElementExecutor<T> _executorGrainReference;

        [NonSerialized] private readonly IElementExecutor<T> _executorReference;

        public IElementExecutor<T> Executor => _executorReference ?? _executorGrainReference;

        public ContainerElementReference(Guid containerId, int offset, IElementExecutor<T> executorReference,
            IElementExecutor<T> executorGrainReference) : base(containerId, offset)
        {
            _executorReference = executorReference;
            _executorGrainReference = executorGrainReference;
        }

        protected bool Equals(ContainerElementReference<T> other)
        {
            return base.Equals(other);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((ContainerElementReference<T>) obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}