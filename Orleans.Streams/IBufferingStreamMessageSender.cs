using System.Threading.Tasks;
using Orleans.Streams.Endpoints;
using Orleans.Streams.Messages;

namespace Orleans.Streams
{
    /// <summary>
    /// Buffers messages and forwards them to the output once the queue buffer reaches a certain size.
    /// </summary>
    public interface IBufferingStreamMessageSender
    {
        /// <summary>
        /// Minimum size that the messages are sent.
        /// </summary>
        int FlushQueueSize { get; set; }

        /// <summary>
        ///     Enqueues a message that is sent to all outputs once the FlushQueueSize is reached.
        /// </summary>
        /// <param name="message">Message to send.</param>
        /// <returns></returns>
        void EnqueueMessageBroadcast(IStreamMessage message);

        /// <summary>
        /// Enqeues a message to one output once the FlushQueueSize is reached.
        /// </summary>
        /// <param name="streamMessage">Message to send.</param>
        void EnqueueMessage(IStreamMessage streamMessage);

        /// <summary>
        /// Awaits all sent messages.
        /// </summary>
        /// <returns></returns>
        Task AwaitSendingComplete();
    }
}