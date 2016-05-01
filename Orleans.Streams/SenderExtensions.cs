using System;
using System.Collections.Generic;
using System.Linq;
using Orleans.Runtime;
using Orleans.Streams.Endpoints;

namespace Orleans.Streams
{
    public static class SenderExtensions
    {
        public static IEnumerable<Tuple<StreamMessageSender<TX>, IList<TY>>> SplitEquallyBetweenSenders<TX, TY>(this List<StreamMessageSender<TX>> senders, IEnumerable<TY> itemsToSplit)
        {
            var itemsPerProvider = (int)Math.Ceiling(itemsToSplit.Count() / (double)senders.Count);
            var chunks = itemsToSplit.BatchIEnumerable(itemsPerProvider);
            return senders.Zip(chunks, (p, c) => new Tuple<StreamMessageSender<TX>, IList<TY>>(p, c));
        }
    }
}