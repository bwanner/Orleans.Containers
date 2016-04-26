using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Orleans.Streams.Messages;

namespace Orleans.Streams
{
    public interface ITransactionalStreamConsumer : ITransactionalStreamTearDown
    {
        Task SubscribeToStreams(IEnumerable<StreamIdentity> inputStreams);

        Task TransactionComplete(Guid transactionId);
    }
}