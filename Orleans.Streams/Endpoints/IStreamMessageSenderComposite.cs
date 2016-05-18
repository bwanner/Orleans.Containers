using System;
using System.Threading.Tasks;
using Orleans.Streams.Messages;

namespace Orleans.Streams.Endpoints
{
    public interface IStreamMessageSenderComposite : IBufferingStreamMessageSender, IStreamMessageSender, ITransactionalStreamProvider
    {
    }

    public interface IStreamMessageSenderComposite<T> : IStreamMessageSenderComposite, ITransactionalStreamProvider<T>
    {
        
    }
}