using System;
using System.Threading.Tasks;
using Orleans.Streams.Messages;

namespace Orleans.Streams.Endpoints
{
    /// <summary>
    ///     Allows to send messages via an output stream.
    /// </summary>
    public interface IStreamMessageSender : ITransactionalStreamProvider
    {
        /// <summary>
        ///     Sends a message to one output.
        /// </summary>
        /// <param name="message">Message to send.</param>
        /// <returns></returns>
        Task SendMessage(IStreamMessage message);

        /// <summary>
        ///     Sends a message to all outputs.
        /// </summary>
        /// <param name="message">Message to send.</param>
        /// <returns></returns>
        Task SendMessageBroadcast(IStreamMessage message);

        /// <summary>
        ///     Starts a new transaction.
        /// </summary>
        /// <param name="transactionId">Identifier.</param>
        /// <returns></returns>
        Task StartTransaction(Guid transactionId);

        /// <summary>
        ///     Ends a transaction.
        /// </summary>
        /// <param name="transactionId">Identifier.</param>
        /// <returns></returns>
        Task EndTransaction(Guid transactionId);
    }

    /// <summary>
    ///     Marker interface.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IStreamMessageSender<T> : IStreamMessageSender, ITransactionalStreamProvider<T>
    {
    }
}