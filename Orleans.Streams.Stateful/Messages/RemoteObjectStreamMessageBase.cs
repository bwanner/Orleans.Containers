using Orleans.Collections;
using Orleans.Streams.Messages;

namespace Orleans.Streams.Stateful.Messages
{
    /// <summary>
    /// Abstract class for messages affecting IRemoteObjectValues.
    /// </summary>
    public abstract class RemoteObjectStreamMessageBase : IStreamMessage
    {
        /// <summary>
        /// Remote value that is affected by this message.
        /// </summary>
        public IObjectRemoteValue ElementAffected { get; private set; }

        protected RemoteObjectStreamMessageBase(IObjectRemoteValue elementAffected)
        {
            ElementAffected = elementAffected;
        }
    }
}