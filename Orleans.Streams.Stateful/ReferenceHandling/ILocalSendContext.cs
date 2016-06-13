using Orleans.Streams.Stateful;

namespace Orleans.Collections
{
    /// <summary>
    /// Context for sending remote objects.
    /// </summary>
    public interface ILocalSendContext
    {
        /// <summary>
        /// Holds linked information to a object that can be sent.
        /// </summary>
        ObjectReferenceLookup<object, object> SendableObjectLookup { get; }
    }
}