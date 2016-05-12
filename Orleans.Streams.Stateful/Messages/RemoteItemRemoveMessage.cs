using System;
using System.Collections.Generic;
using Orleans.Collections;
using Orleans.Streams.Messages;

namespace Orleans.Streams.Stateful.Messages
{
    [Serializable]
    public class RemoteItemRemoveMessage<T> : IStreamMessage
    {
        public IList<IObjectRemoteValue<T>> Items { get; private set; }

        public RemoteItemRemoveMessage(IList<IObjectRemoteValue<T>> items)
        {
            Items = items;
        }
    }
}