using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Orleans.Streams.Messages;

namespace Orleans.Streams.Endpoints
{
    public interface IStreamMessageSender<T> : ITransactionalStreamTearDown, ITransactionalStreamProvider<T>
    {
        Task SendItems(IEnumerable<T> items);
        Task StartTransaction(Guid transactionId);
        Task EndTransaction(Guid transactionId);
        Task SendAddItems(IEnumerable<T> items);
        Task SendUpdateItems(IEnumerable<T> items);
        Task SendRemoveItems(IEnumerable<T> items);
        void EnqueueAddItems(IEnumerable<T> items);
        void EnqueueUpdateItems(IEnumerable<T> items);
        void EnqueueRemoveItems(IEnumerable<T> items);
        void EnqueueMessage(IStreamMessage message);
        Task FlushQueue();
    }
}