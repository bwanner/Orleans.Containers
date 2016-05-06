using System;
using System.Collections.Generic;
using Orleans.Collections.Utilities;

namespace Orleans.Collections
{
    public interface ILocalReceiveContext
    {
        Dictionary<Guid, object> GuidToLocalObjects { get; }

    }
}