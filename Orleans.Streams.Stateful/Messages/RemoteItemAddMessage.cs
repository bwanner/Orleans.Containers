using System;
using System.Collections.Generic;
using Orleans.Collections;
using Orleans.Streams.Messages;

namespace Orleans.Streams.Stateful.Messages
{
    [Serializable]
    public class RemoteItemAddMessage<T> : IStreamMessage
    {
        public IList<IObjectRemoteValue<T>> Items { get; private set; }

        public RemoteItemAddMessage(IList<IObjectRemoteValue<T>> items)
        {
            Items = items;
        }
    }
}