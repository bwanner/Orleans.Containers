using System.Threading.Tasks;

namespace Orleans.Streams.Messages
{
    /// <summary>
    /// Processes a IStreamMessage.
    /// </summary>
    public interface IStreamMessageVisitor
    {
        /// <summary>
        /// Process the stream message.
        /// </summary>
        /// <param name="streamMessage">Message to process.</param>
        /// <returns></returns>
        Task Visit(IStreamMessage streamMessage);
    }
}