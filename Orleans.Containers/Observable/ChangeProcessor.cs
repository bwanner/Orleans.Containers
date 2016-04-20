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
    public abstract class ChangeProcessor<T>
    {
        protected readonly ObjectReferenceCounter<INotifyCollectionChanged> CollectionChangedReferences;
        protected readonly ObjectReferenceCounter<IContainerElementNotifyPropertyChanged> PropertyChangedReferences;  

        public int KnownObjectCount => CollectionChangedReferences.Count + PropertyChangedReferences.Count;

        internal ChangeProcessor()
        {
            CollectionChangedReferences = new ObjectReferenceCounter<INotifyCollectionChanged>();
            PropertyChangedReferences = new ObjectReferenceCounter<IContainerElementNotifyPropertyChanged>();
        }

        public void AddItem(object obj, ObjectIdentityLookup incomingIdentities)
        {
            // TODO Replace identifier duplicates
            // TODO AddToKnownCollectionChangedObjects
            // TODO AddToKnownPropertyChangedObjects
            if (incomingIdentities == null)
                MergeObjectIdentities(obj, true);
            else 
                MergeObjectIdentities(obj, incomingIdentities, true);
        }

        public void AddItems(IEnumerable items, ObjectIdentityLookup incomingIdentities)
        {
            foreach (var item in items)
            {
                AddItem(item, incomingIdentities);
            }
        }

        public void RemoveItem(object obj)
        {
            RemoveObjects(obj, true);
        }

        public void RemoveItems(IEnumerable items)
        {
            foreach (var item in items)
            {
                RemoveItem(item);
            }
        }

        public bool IsKnownObject(ObjectIdentifier identifier)
        {
            return CollectionChangedReferences.ObjectKnown(identifier) || PropertyChangedReferences.ObjectKnown(identifier);
        }


        protected virtual void AddToKnownCollectionChangedObjects(INotifyCollectionChanged list)
        {
            CollectionChangedReferences.IncreaseReferenceCounter(list);
        }

        protected virtual void RemoveFromKnownCollectionChangedObjects(INotifyCollectionChanged collection)
        {
            CollectionChangedReferences.DecreaseReferenceCounter(collection);
        }

        protected virtual void AddToKnownPropertyChangedObjects(IContainerElementNotifyPropertyChanged target)
        {
            PropertyChangedReferences.IncreaseReferenceCounter(target);
        }


        protected virtual void RemoveFromKnownPropertyChangedObjects(IContainerElementNotifyPropertyChanged target)
        {
            PropertyChangedReferences.DecreaseReferenceCounter(target);
        }

        protected bool SupportsPropertyChanged(Type t)
        {
            return t.GetInterfaces().Contains(typeof (IContainerElementNotifyPropertyChanged));
        }

        protected object MergeObjectIdentities(object root, ObjectIdentityLookup identityLookup, bool inspectRoot = false)
        {
            if (root == null)
                return root;

            if (inspectRoot && SupportsPropertyChanged(root.GetType()))
            {
                var rootIdentity = identityLookup.LookupDictionary[root];
                if (IsKnownObject(rootIdentity))
                {
                    PropertyChangedReferences.IncreaseReferenceCounter((IContainerElementNotifyPropertyChanged)root);
                    return PropertyChangedReferences[rootIdentity];
                }
                else
                {
                    PropertyChangedReferences.AddNewItem((IContainerElementNotifyPropertyChanged)root, rootIdentity);
                    // Continue with investigation, do not return
                }
            }


            // Properties implementing IContainerElementNotifyPropertyChanged
            var validProperties = root.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .Where(x => SupportsPropertyChanged(x.PropertyType) && x.GetIndexParameters().Length == 0);

            foreach (var p in validProperties)
            {
                var propertyValue = (IContainerElementNotifyPropertyChanged)p.GetValue(root);
                var propertyIdentity = identityLookup.LookupDictionary[propertyValue];

                if (PropertyChangedReferences.ObjectKnown(propertyIdentity))
                {
                    var localValue = (IContainerElementNotifyPropertyChanged)PropertyChangedReferences[propertyIdentity];
                    PropertyChangedReferences.IncreaseReferenceCounter(localValue);
                    p.SetValue(root, localValue);
                }
                else
                {
                    PropertyChangedReferences.AddNewItem(propertyValue, propertyIdentity);
                    MergeObjectIdentities(propertyValue, identityLookup);
                }
            }

            // Properties annotated with ContainerNotifyCollectionChangedAttribute and implementing IList
            var listProperties =
                root.GetType()
                    .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                    .Where(x => x.GetCustomAttribute<ContainerNotifyCollectionChangedAttribute>() != null && x.GetIndexParameters().Length == 0);

            foreach (var l in listProperties)
            {
                var propertyValue = (INotifyCollectionChanged)l.GetValue(root);
                var propertyIdentity = identityLookup.LookupDictionary[propertyValue];
                if (CollectionChangedReferences.ObjectKnown(propertyIdentity))
                {
                    var localValue = (INotifyCollectionChanged)CollectionChangedReferences[propertyIdentity];
                    CollectionChangedReferences.IncreaseReferenceCounter(localValue);
                    l.SetValue(root, localValue);
                }
                else
                {
                    CollectionChangedReferences.AddNewItem(propertyValue, propertyIdentity);
                    MergeObjectIdentities(propertyValue, identityLookup);
                }
            }

            return root;
        }

        protected object MergeObjectIdentities(object root, bool inspectRoot = false)
        {
            if (inspectRoot && SupportsPropertyChanged(root.GetType()))
            {
                PropertyChangedReferences.IncreaseReferenceCounter((IContainerElementNotifyPropertyChanged) root);
            }


            // Properties implementing IContainerElementNotifyPropertyChanged
            var validProperties = root.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .Where(x => SupportsPropertyChanged(x.PropertyType) && x.GetIndexParameters().Length == 0);

            foreach (var p in validProperties)
            {
                var propertyValue = (IContainerElementNotifyPropertyChanged)p.GetValue(root);
               
                    PropertyChangedReferences.AddNewItem(propertyValue);
                    MergeObjectIdentities(propertyValue);
            }

            // Properties annotated with ContainerNotifyCollectionChangedAttribute and implementing IList
            var listProperties =
                root.GetType()
                    .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                    .Where(x => x.GetCustomAttribute<ContainerNotifyCollectionChangedAttribute>() != null && x.GetIndexParameters().Length == 0);

            foreach (var l in listProperties)
            {
                var propertyValue = (INotifyCollectionChanged)l.GetValue(root);
               
                    CollectionChangedReferences.AddNewItem(propertyValue);
                    MergeObjectIdentities(propertyValue);
            }

            return root;
        }

        protected void RemoveObjects(object root, bool inspectRoot = false)
        {
            if (inspectRoot && SupportsPropertyChanged(root.GetType()))
            {
                PropertyChangedReferences.DecreaseReferenceCounter((IContainerElementNotifyPropertyChanged) root);
            }


            // Properties implementing IContainerElementNotifyPropertyChanged
            var validProperties = root.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .Where(x => SupportsPropertyChanged(x.PropertyType) && x.GetIndexParameters().Length == 0);

            foreach (var p in validProperties)
            {
                var propertyValue = (IContainerElementNotifyPropertyChanged)p.GetValue(root);
                PropertyChangedReferences.DecreaseReferenceCounter(propertyValue);
                RemoveObjects(propertyValue);
            }

            // Properties annotated with ContainerNotifyCollectionChangedAttribute and implementing IList
            var listProperties =
                root.GetType()
                    .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                    .Where(x => x.GetCustomAttribute<ContainerNotifyCollectionChangedAttribute>() != null && x.GetIndexParameters().Length == 0);

            foreach (var l in listProperties)
            {
                var propertyValue = (INotifyCollectionChanged)l.GetValue(root);
                CollectionChangedReferences.DecreaseReferenceCounter(propertyValue);
                RemoveObjects(propertyValue);
            }

        }

    }
}