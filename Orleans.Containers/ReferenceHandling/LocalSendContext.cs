using Orleans.Collections.Utilities;

namespace Orleans.Collections
{
    public class LocalSendContext : ILocalSendContext
    {
        public LocalSendContext()
        {
            SendableObjectLookup = new ObjectReferenceLookup<object, object>();
        }

        public ObjectReferenceLookup<object, object> SendableObjectLookup { get; }
    }
}