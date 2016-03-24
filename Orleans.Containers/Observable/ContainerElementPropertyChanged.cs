using System;

namespace Orleans.Collections.Observable
{
    [Serializable]
    public class ContainerElementPropertyChanged : IContainerElementPropertyChanged
    {
        public ContainerElementPropertyChanged(string propertyName, object value, ObjectIdentifier objectIdentifier)
        {
            PropertyName = propertyName;
            Value = value;
            ObjectIdentifier = objectIdentifier;
        }

        public ObjectIdentifier ObjectIdentifier { get; }
        public string PropertyName { get; private set; }

        public object Value { get; }
    }

    public interface IContainerElementPropertyChanged
    {
        object Value { get; }
        ObjectIdentifier ObjectIdentifier { get; }
        string PropertyName { get; }
    }
}