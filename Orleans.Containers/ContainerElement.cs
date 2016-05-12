using System;

namespace Orleans.Collections
{
    [Serializable]
    public class ContainerElement<T>
    {
        public ContainerElementReference<T> Reference { get; set; }

        public T Item { get; set; }

        public ContainerElement(ContainerElementReference<T> reference, T item)
        {
            Reference = reference;
            Item = item;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((ContainerElement<T>) obj);
        }

        public override int GetHashCode()
        {
            return Reference != null ? Reference.GetHashCode() : 0;
        }

        protected bool Equals(ContainerElement<T> other)
        {
            return Equals(Reference, other.Reference);
        }
    }
}