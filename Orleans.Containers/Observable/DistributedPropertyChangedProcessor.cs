using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Orleans.Streams.Endpoints;
using Orleans.Streams.Messages;

namespace Orleans.Collections.Observable
{
    /// <summary>
    ///     Stores elements in a container. All objects that are added are traversed and properties of type
    ///     IContainerElementNotifyPropertyChanged
    ///     are stored for supporting property changed across containers.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class DistributedPropertyChangedProcessor<T>
    {
        protected readonly Dictionary<ObjectIdentifier, IContainerElementNotifyPropertyChanged> KnownObjects = new Dictionary<ObjectIdentifier, IContainerElementNotifyPropertyChanged>();

        public int KnownObjectCount => KnownObjects.Count;

        public Task ProcessItemPropertyChangedMessage(ItemPropertyChangedMessage arg)
        {
            var matchingObject = KnownObjects[arg.ChangedEventArgs.ObjectIdentifier];
            matchingObject.ApplyChange(arg.ChangedEventArgs);
            return TaskDone.Done;
        }

        public Task ProcessItemMessage(ItemMessage<T> message)
        {
            AddItems(message.Items);
            return TaskDone.Done;
        }

        public void AddItems(IEnumerable<T> items)
        {
            foreach (var item in items)
            {
                ExecuteForElementsWithPropertyChangedSupport(item, AddToKnownObjects);
            }
        }

        public void Clear()
        {
            KnownObjects.Clear();
        }

        public void Remove(object obj)
        {
            ExecuteForElementsWithPropertyChangedSupport(obj, RemoveFromKnownObjects);
        }

        public bool IsKnownObject(ObjectIdentifier identifier)
        {
            return KnownObjects.ContainsKey(identifier);
        }
 
        private void AddToKnownObjects(ObjectIdentifier identifier, IContainerElementNotifyPropertyChanged target)
        {
            KnownObjects.Add(identifier, target);
        }

        private void RemoveFromKnownObjects(ObjectIdentifier identifier, IContainerElementNotifyPropertyChanged target)
        {
            KnownObjects.Remove(identifier);
        }

        private void ExecuteForElementsWithPropertyChangedSupport(object root, Action<ObjectIdentifier, IContainerElementNotifyPropertyChanged> action)
        {
            if (root.GetType().GetInterfaces().Contains(typeof (IContainerElementNotifyPropertyChanged)))
            {
                var casted = (IContainerElementNotifyPropertyChanged) root;
                action(casted.Identifier, casted);
            }

            if (root.GetType().GetInterfaces().Contains(typeof (IEnumerable)))
            {
                foreach (var o in (IEnumerable) root)
                {
                    ExecuteForElementsWithPropertyChangedSupport(o, action);
                }
            }

            var groupedProperties = root.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .GroupBy(x => x.PropertyType.GetInterfaces().Contains(typeof (IContainerElementNotifyPropertyChanged)))
                .ToDictionary(g => g.Key, g => g.ToList());

            if (groupedProperties.ContainsKey(true))
            {
                foreach (var p in groupedProperties[true])
                {
                    if (p.GetIndexParameters().Length != 0)
                    {
                        break;
                    }

                    var propertyValue = (IContainerElementNotifyPropertyChanged) p.GetValue(root);
                    action(propertyValue.Identifier, propertyValue);
                }
            }

            if (groupedProperties.ContainsKey(false))
            {
                foreach (var o in groupedProperties[false])
                {
                    if (o.GetType().Module.ScopeName != "CommonLanguageRuntimeLibrary")
                    {
                        ExecuteForElementsWithPropertyChangedSupport(o, action);
                    }
                }
            }
        }
    }
}