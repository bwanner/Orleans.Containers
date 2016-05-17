using System.Threading.Tasks;
using Orleans.Streams.Endpoints;
using Orleans.Streams.Messages;

namespace Orleans.Streams
{
    public interface IBufferingStreamMessageSender
    {
        int FlushQueueSize { get; set; }

        void EnqueueMessageBroadcast(IStreamMessage streamMessage);

        void EnqueueMessage(IStreamMessage streamMessage);

        Task AwaitSendingComplete();
    }
}