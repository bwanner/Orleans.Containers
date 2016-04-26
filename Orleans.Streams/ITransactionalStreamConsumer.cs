using System;
using System.Threading.Tasks;
using Orleans.Streams.Messages;

namespace Orleans.Streams
{
    public interface ITransactionalStreamConsumer : ITransactionalStreamTearDown
    {
        Task SubscribeToStream(StreamIdentity inputStream);

        Task TransactionComplete(Guid transactionId);
    }
}