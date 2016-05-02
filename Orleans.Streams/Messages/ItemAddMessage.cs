using System;
using System.Collections.Generic;
using Orleans.Streams.Messages;

namespace Orleans.Collections.Messages
{
    /// <summary>
    /// Message containing items that have been recently added by the sender.
    /// </summary>
    /// <typeparam name="T">Type of items</typeparam>
    [Serializable]
    public class ItemAddMessage<T> : ItemMessage<T>
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="items">Items to add.</param>
        public ItemAddMessage(IEnumerable<T> items) : base(items)
        {
        }
    }
}