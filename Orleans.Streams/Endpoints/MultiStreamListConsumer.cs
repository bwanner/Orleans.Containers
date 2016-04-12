using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Orleans.Streams.Endpoints
{
    /// <summary>
    /// Consumes items from multiple streams and places them in a list.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class MultiStreamListConsumer<T> : MultiStreamConsumer<T>
    {
        private Func<T, T, bool> _sameItemFinder;
        public List<T> Items { get; private set; }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="streamProvider">Stream provider to use.</param>
        /// <param name="sameItemFinder">Replaces item if comparison returns true.</param>
        public MultiStreamListConsumer(IStreamProvider streamProvider, Func<T, T, bool> sameItemFinder = null) : base(streamProvider)
        {
            Items = new List<T>();
            StreamItemBatchReceivedFunc = AddItems;
            _sameItemFinder = sameItemFinder;
        }

        private Task AddItems(IEnumerable<T> items)
        {
            if (_sameItemFinder != null)
            {
                foreach (var item in items)
                {
                    var matchingIndex = Items.FindIndex(obj => _sameItemFinder(item, obj));
                    if (matchingIndex >= 0)
                    {
                        Items[matchingIndex] = item;
                    }
                    else
                    {
                        Items.Add(item);
                    }
                }
            }
            else
            {
                Items.AddRange(items);
            }
            return TaskDone.Done;
        }
    }
}