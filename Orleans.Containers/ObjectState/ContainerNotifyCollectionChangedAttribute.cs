using System;
using System.Runtime.CompilerServices;

namespace Orleans.Collections.ObjectState
{
    [Serializable]
    public class ContainerNotifyCollectionChangedAttribute : Attribute
    {
        public string PropertyName { get; private set; }

        public ContainerNotifyCollectionChangedAttribute([CallerMemberName] string propertyName = null)
        {
            PropertyName = propertyName;
        }
    }
}