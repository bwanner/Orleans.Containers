using System;

namespace Orleans.Collections
{
    [Serializable]
    public class ContainerElementAddress<T> : IEquatable<ContainerElementAddress<T>>
    {

        public ContainerElementAddress(Guid containerId, int offset, bool exists)
        {
            ContainerId = containerId;
            Offset = offset;
            Exists = exists;
        }

        public bool Exists { get; }

        public Guid ContainerId { get; }
        public int Offset { get; }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
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

        public bool Equals(ContainerElementAddress<T> other)
        {
            return ContainerId.Equals(other.ContainerId) && Offset == other.Offset;
        }
    }
}