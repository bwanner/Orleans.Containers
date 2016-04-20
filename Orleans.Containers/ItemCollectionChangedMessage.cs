using System;
using Orleans.Collections.Observable;
using Orleans.Streams.Messages;

namespace Orleans.Collections
{
    [Serializable]
    public class ItemCollectionChangedMessage : IStreamMessage
    {
        public ItemCollectionChangedMessage(ContainerElementCollectionChangedEventArgs changedEventArgs)
        {
            ChangedEventArgs = changedEventArgs;
        }

        public ContainerElementCollectionChangedEventArgs ChangedEventArgs { get; set; }
    }
}