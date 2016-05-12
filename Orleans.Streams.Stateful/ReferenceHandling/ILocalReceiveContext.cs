using System;
using System.Collections.Generic;

namespace Orleans.Collections
{
    /// <summary>
    /// Context that is used to map identifiers of remote objects to local objects.
    /// </summary>
    public interface ILocalReceiveContext
    {
        /// <summary>
        /// Lookup for local objects.
        /// </summary>
        Dictionary<Guid, object> GuidToLocalObjects { get; }
    }
}