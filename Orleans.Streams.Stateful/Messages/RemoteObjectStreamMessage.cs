using Orleans.Collections;
using Orleans.Streams.Messages;

namespace Orleans.Streams.Stateful.Messages
{
    public abstract class RemoteObjectStreamMessage : IStreamMessage
    {
        public IObjectRemoteValue ElementAffected { get; private set; }

        public RemoteObjectStreamMessage(IObjectRemoteValue elementAffected)
        {
            ElementAffected = elementAffected;
        }
    }
}