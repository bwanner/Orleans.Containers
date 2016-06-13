using System;

namespace Orleans.Streams
{
    /// <summary>
    ///     Generates transaction IDs.
    /// </summary>
    public static class TransactionGenerator
    {
        /// <summary>
        ///     Generate a transaction ID if not already set.
        /// </summary>
        /// <param name="transactionGuid">Transaction ID to use or null.</param>
        /// <returns>Input transaction ID if not null, random Guid otherwise.</returns>
        public static Guid GenerateTransactionId(Guid? transactionGuid = null)
        {
            return transactionGuid.HasValue ? transactionGuid.Value : Guid.NewGuid();
        }
    }
}