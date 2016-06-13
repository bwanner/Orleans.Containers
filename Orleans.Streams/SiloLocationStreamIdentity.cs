using System;

namespace Orleans.Streams
{
    [Serializable]
    public class SiloLocationStreamIdentity : StreamIdentity
    {
        public string Silo { get; private set; }

        public bool ContainsSilo => Silo != "";

        public SiloLocationStreamIdentity(Guid streamGuid, string streamNamespace, string silo) : base(streamGuid, streamNamespace)
        {
            Silo = silo;
        }
    }
}