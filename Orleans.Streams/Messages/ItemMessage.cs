using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Orleans.Streams.Messages
{
    /// <summary>
    /// Transfer items of a specific type with a message.
    /// </summary>
    /// <typeparam name="T">Type of items to transfer.</typeparam>
    [Serializable]
    public class ItemMessage<T> : IStreamMessage
    {
        /// <summary>
        /// Items contained in message.
        /// </summary>
        public IList<T> Items { get; private set; }

        public ItemMessage(IEnumerable<T> items)
        {
            Items = items.ToList();
        }
    }
}