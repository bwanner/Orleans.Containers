using System.Collections.Specialized;
using Orleans.Collections.ObjectState;

namespace Orleans.Collections.Observable
{
    public class ContainerElementCollectionChangedEventArgs
    {
        public ContainerElementCollectionChangedEventArgs(ObjectIdentifier identifier, object[] items, NotifyCollectionChangedAction action, ObjectIdentityLookup objectIdentityLookup)
        {
            Identifier = identifier;
            Items = items;
            Action = action;
            IdentityLookup = objectIdentityLookup;
        }

        public ObjectIdentityLookup IdentityLookup { get; private set; }

        public ObjectIdentifier Identifier { get; private set; }

        public object[] Items { get; private set; }

        public NotifyCollectionChangedAction Action { get; private set; }
    }
}