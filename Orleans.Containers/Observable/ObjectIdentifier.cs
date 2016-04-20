using System;

namespace Orleans.Collections.Observable
{
    [Serializable]
    public class ObjectIdentifier
    {
        public readonly long SequentialId;
        public readonly Guid ObjectId;

        public ObjectIdentifier(long sequentialId, Guid objectId)
        {
            SequentialId = sequentialId;
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
                return (SequentialId.GetHashCode()*397) ^ ObjectId.GetHashCode();
            }
        }

        protected bool Equals(ObjectIdentifier other)
        {
            return SequentialId.Equals(other.SequentialId) && ObjectId.Equals(other.ObjectId);
        }
    }
}