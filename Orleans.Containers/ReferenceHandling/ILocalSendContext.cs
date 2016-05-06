using Orleans.Collections.Utilities;

namespace Orleans.Collections
{
    public interface ILocalSendContext
    {
        ObjectReferenceLookup<object, object> SendableObjectLookup { get; }
    }
}