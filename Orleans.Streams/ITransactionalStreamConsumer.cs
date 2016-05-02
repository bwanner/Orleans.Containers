using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Orleans.Streams.Messages;

namespace Orleans.Streams
{
    /// <summary>
    /// Allows subscription to a transactional stream.
    /// </summary>
    public interface ITransactionalStreamConsumer : ITransactionalStreamTearDown
    {
        /// <summary>
        /// Subscribe to passed streams.
        /// </summary>
        /// <param name="inputStreams">Streams to subscribe to.</param>
        /// <returns></returns>
        Task SubscribeToStreams(IEnumerable<StreamIdentity> inputStreams);

        /// <summary>
        /// Returns if transaction is completed.
        /// </summary>
        /// <param name="transactionId">Transaction ID to check.</param>
        /// <returns>Only returns when transaction is complete.</returns>
        Task TransactionComplete(Guid transactionId);
    }
}