using System;
using System.Collections.Generic;
using Orleans.Collections.Utilities;

namespace Orleans.Collections
{
    public class LocalReceiveContext : ILocalReceiveContext
    {
        public LocalReceiveContext()
        {
            GuidToLocalObjects = new Dictionary<Guid, object>();
        }

        public Dictionary<Guid, object> GuidToLocalObjects { get; }
    }
}