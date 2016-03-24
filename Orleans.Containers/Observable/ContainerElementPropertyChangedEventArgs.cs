using System;

namespace Orleans.Collections.Observable
{
    [Serializable]
    public class ContainerElementPropertyChangedEventArgs
    {
        public ContainerElementPropertyChangedEventArgs(string propertyName, object value, ObjectIdentifier objectIdentifier)
        {
            PropertyName = propertyName;
            Value = value;
            ObjectIdentifier = objectIdentifier;
        }

        public ObjectIdentifier ObjectIdentifier { get; }
        public string PropertyName { get; private set; }

        public object Value { get; }
    }
}