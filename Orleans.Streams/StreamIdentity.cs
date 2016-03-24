using System;

namespace Orleans.Streams
{
    /// <summary>
    /// Stores information about all transactional streams available.
    /// </summary>
    [Serializable]
    public class StreamIdentity
    {
        private const string NamespacePostfix = "MessageStream";

        public Tuple<Guid, string> StreamIdentifier { get; private set; }


        public StreamIdentity(string namespacePrefix, Guid streamIdentifier)
        {
            StreamIdentifier = new Tuple<Guid, string>(streamIdentifier, namespacePrefix + NamespacePostfix);
        }
    }
}