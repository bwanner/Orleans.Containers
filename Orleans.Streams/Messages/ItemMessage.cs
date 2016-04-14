using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Orleans.Streams.Messages
{
    [Serializable]
    public abstract class ItemMessage<T> : IStreamMessage
    {
        public IList<T> Items { get; private set; }

        protected ItemMessage(IEnumerable<T> items)
        {
            Items = items.ToList();
        }
    }
}