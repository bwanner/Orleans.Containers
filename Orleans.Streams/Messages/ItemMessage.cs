using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Orleans.Streams.Messages
{
    [Serializable]
    public class ItemMessage<T> : IStreamMessage
    {
        public IEnumerable<T> Items { get; private set; }

        public ItemMessage(IEnumerable<T> items)
        {
            Items = items;
        }
    }
}