using System;
using System.Runtime.CompilerServices;
using Orleans.Collections.Observable;

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

    public delegate EventHandler<ContainerElementCollectionChangedEventArgs> ContainerElementCollectionChangedEventHandler();
}