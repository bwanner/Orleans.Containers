using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using Orleans.Collections.ObjectState;

namespace Orleans.Collections.Observable
{
    public abstract class ChangeProcessor
    {
        protected readonly ObjectReferenceCounter ObjectReferences;

        public int KnownObjectCount => ObjectReferences.Count;

        internal ChangeProcessor()
        {
            ObjectReferences = new ObjectReferenceCounter();
        }

        public abstract object AddItem(object obj, ObjectIdentityLookup incomingIdentities = null);

        public IEnumerable<T> AddItems<T>(IEnumerable items, ObjectIdentityLookup incomingIdentities = null)
        {
            return (from object item in items select (T) AddItem(item, incomingIdentities)).ToList();
        }

        public abstract void RemoveItem(object obj);

        public void RemoveItems(IEnumerable items)
        {
            foreach (var item in items)
            {
                RemoveItem(item);
            }
        }

        public bool IsKnownObject(ObjectIdentifier identifier)
        {
            return ObjectReferences.ObjectKnown(identifier);
        }

        protected bool SupportsPropertyChanged(Type t)
        {
            return t.GetInterfaces().Contains(typeof (IContainerElementNotifyPropertyChanged));
        }

        /// <summary>
        /// Get Properties implementing IContainerElementNotifyPropertyChanged
        /// </summary>
        /// <param name="root"></param>
        /// <returns></returns>
        protected IEnumerable<PropertyInfo> GetDirectPropertyChangedTypes(object root)
        {
            return root.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .Where(x => SupportsPropertyChanged(x.PropertyType) && x.GetIndexParameters().Length == 0);
        }


        /// <summary>
        /// Get properties annotated with ContainerNotifyCollectionChangedAttribute and implementing IList
        /// </summary>
        /// <param name="root"></param>
        /// <returns></returns>
        protected IEnumerable<PropertyInfo> GetDirectCollectionChangedTypes(object root)
        {
            return root.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .Where(x => x.GetCustomAttribute<ContainerNotifyCollectionChangedAttribute>() != null && x.GetIndexParameters().Length == 0);
        }

    }
}