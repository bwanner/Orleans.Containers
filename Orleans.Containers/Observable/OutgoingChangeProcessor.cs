using System;
using System.Collections.Specialized;
using System.ComponentModel;
using Orleans.Collections.ObjectState;

namespace Orleans.Collections.Observable
{
    public class OutgoingChangeProcessor : ChangeProcessor
    {
        public event ContainerElementPropertyChangedEventHandler ContainerPropertyChanged;

        public event ContainerElementCollectionChangedEventHandler ContainerCollectionChanged;


        public override object AddItem(object obj, ObjectIdentityLookup incomingIdentities)
        {
            throw new NotImplementedException();
        }

        public override void RemoveItem(object obj)
        {
            throw new NotImplementedException();
        }
    }
}