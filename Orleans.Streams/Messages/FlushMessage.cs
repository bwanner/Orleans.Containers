using System;

namespace Orleans.Streams.Messages
{
    /// <summary>
    /// Message sent to trigger flushing the pipeline. Has to be forwarded from each stream processor to its output stream.
    /// </summary>
    [Serializable]
    public class FlushMessage : IStreamMessage
    {
        
    }
}