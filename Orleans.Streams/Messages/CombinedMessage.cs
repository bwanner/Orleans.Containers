using System;
using System.Collections.Generic;

namespace Orleans.Streams.Messages
{
    /// <summary>
    /// A message combining multiple IStreamMessages.
    /// </summary>
    [Serializable]
    public class CombinedMessage : IStreamMessage
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="messages"></param>
        public CombinedMessage(List<IStreamMessage> messages)
        {
            Messages = messages;
        }

        /// <summary>
        /// Contained messages.
        /// </summary>
        public List<IStreamMessage> Messages { get; }
    }
}