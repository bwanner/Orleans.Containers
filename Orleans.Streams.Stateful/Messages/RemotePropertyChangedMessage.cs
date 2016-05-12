using System;
using Orleans.Collections;

namespace Orleans.Streams.Stateful.Messages
{
    [Serializable]
    public class RemotePropertyChangedMessage : RemoteObjectStreamMessage
    {
        public IObjectRemoteValue Value { get; private set; }
        public string PropertyName { get; private set; }
        public IObjectRemoteValue OldValue { get; set; }

        public RemotePropertyChangedMessage(IObjectRemoteValue value, IObjectRemoteValue oldValue, IObjectRemoteValue elementAffected, string propertyName)
            : base(elementAffected)
        {
            Value = value;
            OldValue = oldValue;
            PropertyName = propertyName;
        }
    }
}