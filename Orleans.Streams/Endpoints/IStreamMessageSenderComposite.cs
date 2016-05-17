using System;
using System.Threading.Tasks;
using Orleans.Streams.Messages;

namespace Orleans.Streams.Endpoints
{
    public interface IStreamMessageSenderComposite : IBufferingStreamMessageSender, ITransactionalStreamProvider
    {
        /// <summary>
        ///     Sends a message on one output.
        /// </summary>
        /// <param name="message">Message to send.</param>
        /// <returns></returns>
        Task SendMessage(IStreamMessage message);

        /// <summary>
        /// Sends a message through all available outputs.
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        Task SendMessageThroughAllOutputs(IStreamMessage message);

        Task StartTransaction(Guid transactionId);

        Task EndTransaction(Guid transactionId);
    }

    public interface IStreamMessageSenderComposite<T> : IStreamMessageSenderComposite, ITransactionalStreamProvider<T>
    {
        
    }
}