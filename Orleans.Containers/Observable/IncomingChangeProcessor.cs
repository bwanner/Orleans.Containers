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
    public class IncomingChangeProcessor<T> : ChangeProcessor<T>
    {
        public Task ProcessItemPropertyChangedMessage(ItemPropertyChangedMessage arg)
        {
            var matchingObject = PropertyChangedReferences[arg.ChangedEventArgs.Identifier];
            var updatedValue = MergeObjectIdentities(arg.ChangedEventArgs.Value, arg.ChangedEventArgs.IdentityLookup, true);

            var oldValue = matchingObject.ApplyChange(arg.ChangedEventArgs.PropertyName, updatedValue);
            RemoveObjects(oldValue, true);

            return TaskDone.Done;
        }


        public Task<IEnumerable<T>> ProcessItemAddMessage(ObservableItemAddMessage<T> message)
        {
            var localItems = AddItems(message.Items, message.Identities);
            return Task.FromResult((IEnumerable<T>) localItems);
        }

        public Task<IEnumerable<T>> ProcessItemAddMessage(ItemAddMessage<T> message)
        {
            var localItems = AddItems(message.Items, null);
            return Task.FromResult((IEnumerable<T>) localItems);
        }

        public Task ProcessItemRemoveMessage(ItemRemoveMessage<T> message)
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
            var matchingCollection = CollectionChangedReferences[message.ChangedEventArgs.Identifier];
            switch (arg.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (var item in arg.Items)
                    {
                        var updatedValue = MergeObjectIdentities(item, arg.IdentityLookup, true);
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
    }
}