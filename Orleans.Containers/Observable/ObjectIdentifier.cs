using System;

namespace Orleans.Collections.Observable
{
    [Serializable]
    public class ObjectIdentifier
    {
        public readonly long ObjectId;

        public ObjectIdentifier(long objectId)
        {
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
                return ObjectId.GetHashCode();
            }
        }

        protected bool Equals(ObjectIdentifier other)
        {
            return ObjectId.Equals(other.ObjectId);
        }
    }
}