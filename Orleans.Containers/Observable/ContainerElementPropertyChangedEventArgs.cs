using System;
using Orleans.Collections.ObjectState;

namespace Orleans.Collections.Observable
{
    [Serializable]
    public class ContainerElementPropertyChangedEventArgs
    {
        public ContainerElementPropertyChangedEventArgs(string propertyName, object value, ObjectIdentifier identifier, ObjectIdentityLookup objectIdentityLookup)
        {
            PropertyName = propertyName;
            Value = value;
            Identifier = identifier;
            IdentityLookup = objectIdentityLookup;
        }

        public ObjectIdentityLookup IdentityLookup { get; private set; }

        public ObjectIdentifier Identifier { get; }
        public string PropertyName { get; private set; }

        public object Value { get; }
    }
}