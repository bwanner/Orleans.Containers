using System;
using System.Collections.Generic;

namespace Orleans.Collections
{
    /// <summary>
    /// Default local receive context.
    /// </summary>
    public class LocalReceiveContext : ILocalReceiveContext
    {
        /// <summary>
        /// Create a new LocalReceiveContext.
        /// </summary>
        public LocalReceiveContext()
        {
            GuidToLocalObjects = new Dictionary<Guid, object>();
        }

        /// <summary>
        /// Lookup for local objects.
        /// </summary>
        public Dictionary<Guid, object> GuidToLocalObjects { get; }
    }
}