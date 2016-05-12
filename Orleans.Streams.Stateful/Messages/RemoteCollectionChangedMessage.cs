using System;
using System.Collections.Specialized;
using Orleans.Collections;

namespace Orleans.Streams.Stateful.Messages
{
    [Serializable]
    public class RemoteCollectionChangedMessage : RemoteObjectStreamMessage
    {
        public NotifyCollectionChangedAction Action { get; private set; }
        public IObjectRemoteValue[] Elements { get; private set; }

        public RemoteCollectionChangedMessage(NotifyCollectionChangedAction action, IObjectRemoteValue sourceElement, IObjectRemoteValue[] elements) : base(sourceElement)
        {
            Action = action;
            Elements = elements;
        }
    }
}