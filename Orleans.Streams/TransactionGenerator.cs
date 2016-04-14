using System;

namespace Orleans.Streams
{
    public static class TransactionGenerator
    {
        public static Guid GenerateTransactionId(Guid? transactionGuid = null)
        {
            return (transactionGuid.HasValue) ? transactionGuid.Value : Guid.NewGuid();
        }
    }
}