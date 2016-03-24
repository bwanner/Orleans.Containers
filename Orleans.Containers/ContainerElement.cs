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
    }
}