using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Orleans.Collections.Observable;

namespace Orleans.Collections.ObjectState
{
    public class ObjectIdentityLookup
    {
        private Dictionary<object, ObjectIdentifier> _lookup;

        public Dictionary<object, ObjectIdentifier> LookupDictionary => _lookup;

        public ObjectIdentityLookup()
        {
          _lookup = new Dictionary<object, ObjectIdentifier>(); 
        }
    }
}