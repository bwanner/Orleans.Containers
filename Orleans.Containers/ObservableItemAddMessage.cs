using System;
using System.Collections.Generic;
using Orleans.Collections.Messages;
using Orleans.Collections.ObjectState;

namespace Orleans.Collections
{
    [Serializable]
    public class ObservableItemAddMessage<T> : ItemAddMessage<T>
    {
        public ObservableItemAddMessage(IEnumerable<T> items, ObjectIdentityLookup identityLookup) : base(items)
        {
            Identities = identityLookup;
        }

        public ObjectIdentityLookup Identities { get; set; }
    }
}