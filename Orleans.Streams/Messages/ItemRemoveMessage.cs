using System;
using System.Collections.Generic;
using Orleans.Streams.Messages;

namespace Orleans.Collections.Messages
{
    /// <summary>
    /// Message containing items that have been recently removed by the sender.
    /// </summary>
    /// <typeparam name="T">Type of items</typeparam>
    [Serializable]
    public class ItemRemoveMessage<T> : ItemMessage<T>
    {
        public ItemRemoveMessage(IEnumerable<T> items) : base(items)
        {
        }
    }
}