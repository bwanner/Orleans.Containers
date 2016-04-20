using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Orleans.Collections.Messages;
using Orleans.Collections.ObjectState;

namespace Orleans.Collections.Observable
{
    /// <summary>
    /// Processes incoming items or changes to them. Items are replaced by their local copies represented through the same ObjectIdentifier if available.
    /// Otherwise they are remembered so updates on object level can be provided for:
    ///     - Classes implementing IContainerElementNotifyPropertyChanged
    ///     - IList and INotifyCollectionChanged implementing objects (e.g. ObservableCollection) marked with ContainerNotifyCollectionChangedAttribute.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class IncomingChangeProcessor : ChangeProcessor
    {
        public Task ProcessItemPropertyChangedMessage(ItemPropertyChangedMessage arg)
        {
            var matchingObject = (IContainerElementNotifyPropertyChanged) ObjectReferences[arg.ChangedEventArgs.Identifier];
            var updatedValue = AddObjectToLocalScope(arg.ChangedEventArgs.Value, arg.ChangedEventArgs.IdentityLookup);

            var oldValue = matchingObject.ApplyChange(arg.ChangedEventArgs.PropertyName, updatedValue);
            RemoveObjects(oldValue);

            return TaskDone.Done;
        }


        public Task<List<T>> ProcessItemAddMessage<T>(ObservableItemAddMessage<T> message)
        {
            var localItems = AddItems<T>(message.Items, message.Identities);
            return Task.FromResult(localItems.ToList());
        }

        public Task<IEnumerable<T>> ProcessItemAddMessage<T>(ItemAddMessage<T> message)
        {
            var localItems = AddItems<T>(message.Items, null);
            return Task.FromResult((IEnumerable<T>) localItems);
        }

        public Task ProcessItemRemoveMessage<T>(ItemRemoveMessage<T> message)
        {
            foreach (var item in message.Items)
            {
                RemoveItem(item);
            }

            return TaskDone.Done;
        }

        public Task ProcessItemCollectionChangedMessage(ItemCollectionChangedMessage message)
        {
            var arg = message.ChangedEventArgs;
            var matchingCollection = ObjectReferences[message.ChangedEventArgs.Identifier];
            switch (arg.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (var item in arg.Items)
                    {
                        var updatedValue = AddObjectToLocalScope(item, arg.IdentityLookup);
                        ((IList) matchingCollection).Add(updatedValue);
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                case NotifyCollectionChangedAction.Reset:
                    foreach (var item in arg.Items)
                    {
                        var deleteIdentifier = arg.IdentityLookup.LookupDictionary[item];
                        ((IList) matchingCollection).Remove(item);
                    }
                    RemoveItems(arg.Items);
                    break;
                default:
                    throw new NotImplementedException();
            }

            return TaskDone.Done;
        }


        protected object AddObjectToLocalScope(object root, ObjectIdentityLookup identityLookup = null, bool rootIsNotifyCollectionChanged = false)
        {
            if (root == null)
                return root;

            if (rootIsNotifyCollectionChanged || SupportsPropertyChanged(root.GetType()))
            {
                var rootIdentity = identityLookup?.LookupDictionary[root];
                if (IsKnownObject(rootIdentity))
                {
                    ObjectReferences.IncreaseReferenceCounter(root);
                    return ObjectReferences[rootIdentity];
                }
                else
                {
                    ObjectReferences.AddNewItem(root, rootIdentity);
                    if (rootIsNotifyCollectionChanged)
                    {
                        foreach (var item in (IList) root)
                        {
                            AddObjectToLocalScope(item, identityLookup);
                        }
                    }
                }
                    // Continue with investigation, do not return
            }

            foreach (var p in GetDirectPropertyChangedTypes(root).Concat(GetDirectCollectionChangedTypes(root)))
            {
                var propertyValue = p.GetValue(root);
                var propertyIdentity = identityLookup?.LookupDictionary[propertyValue];

                if (ObjectReferences.ObjectKnown(propertyIdentity))
                {
                    var localValue = ObjectReferences[propertyIdentity];
                    ObjectReferences.IncreaseReferenceCounter(localValue);
                    p.SetValue(root, localValue);
                }
                else
                {
                    AddObjectToLocalScope(propertyValue, identityLookup, propertyValue is INotifyCollectionChanged);
                }
            }

            return root;
        }

        protected void RemoveObjects(object root)
        {
            if (root == null)
                return;
            if (SupportsPropertyChanged(root.GetType()))
            {
                if(!ObjectReferences.DecreaseReferenceCounter((IContainerElementNotifyPropertyChanged)root))
                    return;
            }


            // Properties implementing IContainerElementNotifyPropertyChanged
            var validProperties = root.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .Where(x => SupportsPropertyChanged(x.PropertyType) && x.GetIndexParameters().Length == 0);

            foreach (var p in validProperties)
            {
                var propertyValue = (IContainerElementNotifyPropertyChanged)p.GetValue(root);

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
                ObjectReferences.DecreaseReferenceCounter(propertyValue);
                foreach (var includedItem in ((IList)propertyValue))
                {
                    RemoveObjects(includedItem);
                }
            }
        }

        public override object AddItem(object obj, ObjectIdentityLookup incomingIdentities)
        {
            // TODO Replace identifier duplicates
            // TODO AddToKnownCollectionChangedObjects
            // TODO AddToKnownPropertyChangedObjects
            return AddObjectToLocalScope(obj, incomingIdentities);
        }

        public override void RemoveItem(object obj)
        {
            RemoveObjects(obj);
        }
    }
}