using System;
using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using Orleans.Collections.ObjectState;

namespace Orleans.Collections.Observable
{
    public class OutgoingChangeProcessor : ChangeProcessor
    {
        public event ContainerElementPropertyChangedEventHandler ContainerPropertyChanged;

        public event ContainerElementCollectionChangedEventHandler ContainerCollectionChanged;


        public override object AddItem(object obj, ObjectIdentityLookup incomingIdentities = null)
        {
            AddObjectToLocalScope(obj);
            return obj;
        }

        public override void RemoveItem(object obj)
        {
            throw new NotImplementedException();
        }

        protected void AddObjectToLocalScope(object root, bool rootIsNotifyCollectionChanged = false)
        {
            if (root == null)
                return;

            if (rootIsNotifyCollectionChanged || SupportsPropertyChanged(root.GetType()))
            {
                var rootIdentity = ObjectIdentityGenerator.Instance.GetId(root);
                if (!IsKnownObject(rootIdentity))
                {
                    ObjectReferences.AddNewItem(root);
                    if (rootIsNotifyCollectionChanged)
                    {
                        ((INotifyCollectionChanged) root).CollectionChanged += OutgoingChangeProcessor_CollectionChanged;
                        foreach (var item in (IList) root)
                        {
                            AddObjectToLocalScope(item);
                        }
                    }
                    else
                    {
                        ((INotifyPropertyChanged) root).PropertyChanged += OutgoingChangeProcessor_PropertyChanged;
                    }
                }
                // Continue with investigation, do not return
            }

            foreach (var p in GetDirectPropertyChangedTypes(root).Concat(GetDirectCollectionChangedTypes(root)))
            {
                var propertyValue = p.GetValue(root);
                var propertyIdentity = ObjectIdentityGenerator.Instance.GetId(propertyValue);

                if (!ObjectReferences.ObjectKnown(propertyIdentity))
                {
                    AddObjectToLocalScope(propertyValue, propertyValue is INotifyCollectionChanged);
                }
            }
        }

        protected void GetObjectIdentifierRecursive(object root, ObjectIdentityLookup identityLookup, bool rootIsNotifyCollectionChanged = false)
        {
            if (root == null)
                return;

            if (rootIsNotifyCollectionChanged || SupportsPropertyChanged(root.GetType()))
            {
                var rootIdentity = ObjectIdentityGenerator.Instance.GetId(root);
                identityLookup.LookupDictionary.Add(root, rootIdentity);
                if (rootIsNotifyCollectionChanged)
                {
                    foreach (var item in (IList) root)
                    {
                        GetObjectIdentifierRecursive(item, identityLookup);
                    }
                }
                // Continue with investigation, do not return
            }

            foreach (var p in GetDirectPropertyChangedTypes(root).Concat(GetDirectCollectionChangedTypes(root)))
            {
                var propertyValue = p.GetValue(root);
                GetObjectIdentifierRecursive(propertyValue, identityLookup, propertyValue is INotifyCollectionChanged);
            }
        }


        private void OutgoingChangeProcessor_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void OutgoingChangeProcessor_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var newValue = sender.GetType().GetProperty(e.PropertyName).GetGetMethod(true).Invoke(sender, null);
            ObjectIdentityLookup identityLookup = new ObjectIdentityLookup();
            GetObjectIdentifierRecursive(newValue, identityLookup);
            var containerPropertyChangedArgs = new ContainerElementPropertyChangedEventArgs(e.PropertyName, newValue,
                ObjectIdentityGenerator.Instance.GetId(sender), identityLookup);

            ContainerPropertyChanged?.Invoke(containerPropertyChangedArgs);
        }
    }
}