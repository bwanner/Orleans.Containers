using System;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using Orleans.Collections.Observable;

namespace Orleans.Collections.ObjectState
{
    public class ObjectIdentityGenerator {

        public static ObjectIdentityGenerator Instance => _instance ?? (_instance = new ObjectIdentityGenerator());

        private readonly ObjectIDGenerator _generator = new ObjectIDGenerator();
        private readonly ConditionalWeakTable<object, ObjectIdentifier> _weakTable = new ConditionalWeakTable<object, ObjectIdentifier>();
        private static ObjectIdentityGenerator _instance;

        public ObjectIdentifier GetId(object obj)
        {
            ObjectIdentifier identifier = null;
            if (!_weakTable.TryGetValue(obj, out identifier))
            {
                bool firstTime = false;
                identifier = new ObjectIdentifier(_generator.GetId(obj, out firstTime));
                _weakTable.Add(obj, identifier);
            }

            return identifier;
        }

        public void SetId(ObjectIdentifier identifier, object obj)
        {
            _weakTable.Add(obj, identifier);
        }
    }
}