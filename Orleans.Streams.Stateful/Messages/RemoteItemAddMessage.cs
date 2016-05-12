using System;
using System.Collections.Generic;
using Orleans.Streams.Messages;

namespace Orleans.Streams.Stateful.Messages
{
    /// <summary>
    /// Message to notify that remote items have been added.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [Serializable]
    public class RemoteItemAddMessage<T> : IStreamMessage
    {
        /// <summary>
        /// Remote items that have been added.
        /// </summary>
        public IList<IObjectRemoteValue<T>> Items { get; private set; }

        /// <summary>
        /// Create a new RemoteItemAddMessage.
        /// </summary>
        /// <param name="items">Items that have been added.</param>
        public RemoteItemAddMessage(IList<IObjectRemoteValue<T>> items)
        {
            Items = items;
        }
    }
}