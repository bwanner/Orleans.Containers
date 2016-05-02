using System;

namespace Orleans.Streams
{
    /// <summary>
    ///     Stores information about all transactional streams available.
    /// </summary>
    [Serializable]
    public class StreamIdentity
    {
        private const string NamespacePostfix = "MessageStream";

        /// <summary>
        /// Identifier for a stream.
        /// </summary>
        public Tuple<Guid, string> StreamIdentifier { get; private set; }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="namespacePrefix"></param>
        /// <param name="streamIdentifier"></param>
        public StreamIdentity(string namespacePrefix, Guid streamIdentifier)
        {
            StreamIdentifier = new Tuple<Guid, string>(streamIdentifier, namespacePrefix + NamespacePostfix);
        }
    }
}