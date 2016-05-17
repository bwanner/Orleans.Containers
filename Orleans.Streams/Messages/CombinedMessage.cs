using System;
using System.Collections.Generic;

namespace Orleans.Streams.Messages
{
    [Serializable]
    public class CombinedMessage : IStreamMessage
    {
        public CombinedMessage(List<IStreamMessage> messages)
        {
            Messages = messages;
        }

        public List<IStreamMessage> Messages { get; }
    }
}