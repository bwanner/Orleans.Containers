using System;
using System.Collections.Generic;
using Orleans.Streams.Messages;

namespace Orleans.Streams.Stateful.Messages
{
    /// <summary>
    /// Message to notify of multiple removed remote objects.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [Serializable]
    public class RemoteItemRemoveMessage<T> : IStreamMessage
    {
        /// <summary>
        /// Remote objects that have been removed.
        /// </summary>
        public IList<IObjectRemoteValue<T>> Items { get; private set; }

        /// <summary>
        /// Create a new RemoteItemRemoveMessage.
        /// </summary>
        /// <param name="items">Remote objects that have been removed.</param>
        public RemoteItemRemoveMessage(IList<IObjectRemoteValue<T>> items)
        {
            Items = items;
        }
    }
}