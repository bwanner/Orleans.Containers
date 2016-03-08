using System;
using System.Threading.Tasks;

namespace Orleans.Streams
{
    public interface ITransactionalStreamConsumer<TIn> : ITransactionalStreamTearDown
    {
        Task SetInput(TransactionalStreamIdentity<TIn> inputStream);

        Task TransactionComplete(int transactionId);
    }
}