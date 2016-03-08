using System;

namespace Orleans.Streams
{
    /// <summary>
    /// Stores information about all transactional streams available.
    /// </summary>
    /// <typeparam name="TItem"></typeparam>
    [Serializable]
    public class TransactionalStreamIdentity<TItem>
    {
        private const string ItemStreamBatchNamespacePostfix = "ItemBatchStream";
        private const string TransactionStreamNamespacePostfix = "TransactionStream";

        public Tuple<Guid, string> TransactionStreamIdentifier { get; private set; }

        public Tuple<Guid, string> ItemBatchStreamIdentifier { get; private set; }

        public TransactionalStreamIdentity(string namespacePrefix, Guid streamIdentifier)
        {
            TransactionStreamIdentifier = new Tuple<Guid, string>(streamIdentifier, namespacePrefix + TransactionStreamNamespacePostfix);
            ItemBatchStreamIdentifier = new Tuple<Guid, string>(streamIdentifier, namespacePrefix + ItemStreamBatchNamespacePostfix);
        }
    }
}