using System;

namespace Orleans.Collections.Observable
{
    public class ObjectIdentifier
    {
        public readonly Guid ContainerId;
        public readonly Guid ObjectId;

        public ObjectIdentifier(Guid containerId, Guid objectId)
        {
            ContainerId = containerId;
            ObjectId = objectId;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((ObjectIdentifier) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (ContainerId.GetHashCode()*397) ^ ObjectId.GetHashCode();
            }
        }

        protected bool Equals(ObjectIdentifier other)
        {
            return ContainerId.Equals(other.ContainerId) && ObjectId.Equals(other.ObjectId);
        }
    }
}