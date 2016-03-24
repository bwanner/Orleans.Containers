using System;
using Orleans.Collections.Observable;
using Orleans.Streams.Messages;

namespace Orleans.Collections
{
    [Serializable]
    public class ItemPropertyChangedMessage : IStreamMessage
    {
        public ItemPropertyChangedMessage(ContainerElementPropertyChangedEventArgs changedEventArgs)
        {
            ChangedEventArgs = changedEventArgs;
        }

        public ContainerElementPropertyChangedEventArgs ChangedEventArgs { get; set; }
    }
}