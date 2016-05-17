using System;
using System.Collections.Generic;
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
        ///     Sends a generic message on all outputs.
        /// </summary>
        /// <param name="message">Message to send.</param>
        /// <returns></returns>
        Task SendMessage(IStreamMessage message);

        Task StartTransaction(Guid transactionId);

        Task EndTransaction(Guid transactionId);
    }

    /// <summary>
    /// Marker interface.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IStreamMessageSender<T> : IStreamMessageSender, ITransactionalStreamProvider<T>
    {
        
    }
}