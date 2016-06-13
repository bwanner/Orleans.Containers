using Orleans.Collections;

namespace Orleans.Streams.Stateful
{
    /// <summary>
    /// Simple LocalSendContext.
    /// </summary>
    public class LocalSendContext : ILocalSendContext
    {
        /// <summary>
        /// Create a new LocalSendContext.
        /// </summary>
        public LocalSendContext()
        {
            SendableObjectLookup = new ObjectReferenceLookup<object, object>();
        }

        /// <summary>
        /// Holds linked information to a object that can be sent.
        /// </summary>
        public ObjectReferenceLookup<object, object> SendableObjectLookup { get; }
    }
}