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
            IElementExecutor<T> executorGrainReference, bool exists) : base(containerId, offset, exists)
        {
            _executorReference = executorReference;
            _executorGrainReference = executorGrainReference;
        }
    }
}