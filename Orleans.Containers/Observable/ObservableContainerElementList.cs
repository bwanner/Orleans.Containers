using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Orleans.Streams.Endpoints;

namespace Orleans.Collections.Observable
{
    /// <summary>
    ///     Stores elements in a container. All objects that are added are traversed and properties of type
    ///     IContainerElementNotifyPropertyChanged
    ///     are stored for supporting property changed across containers.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ObservableContainerElementList<T> : ContainerElementList<T>
    {
        private readonly Dictionary<ObjectIdentifier, IContainerElementNotifyPropertyChanged> _knownObjects = new Dictionary<ObjectIdentifier, IContainerElementNotifyPropertyChanged>();

        public int KnownObjectCount => _knownObjects.Count;

        public ObservableContainerElementList(Guid containerId, IElementExecutor<T> executorReference, IElementExecutor<T> executorGrainReference,
            StreamMessageDispatchReceiver dispatchReceiver = null)
            : base(containerId, executorReference, executorGrainReference)
        {
            dispatchReceiver?.Register<ItemPropertyChangedMessage>(ProcessItemPropertyChangedMessage);
        }

        private Task ProcessItemPropertyChangedMessage(ItemPropertyChangedMessage arg)
        {
            var matchingObject = _knownObjects[arg.ChangedEventArgs.ObjectIdentifier];
            matchingObject.ApplyChange(arg.ChangedEventArgs);
            return TaskDone.Done;
        }

        public override async Task<IReadOnlyCollection<ContainerElementReference<T>>> AddRange(IEnumerable<T> items)
        {
            foreach (var item in items)
            {
                ExecuteForElementsWithPropertyChangedSupport(item, AddToKnownObjects);
            }

            return await base.AddRange(items);
        }

        public override Task Clear()
        {
            _knownObjects.Clear();
            return base.Clear();
        }

        public override async Task<bool> Remove(ContainerElementReference<T> reference)
        {
            var obj = this[reference];
            ExecuteForElementsWithPropertyChangedSupport(obj, RemoveFromKnownObjects);
            return await base.Remove(reference);
        }

        public bool IsKnownObject(ObjectIdentifier identifier)
        {
            return _knownObjects.ContainsKey(identifier);
        }

        private void AddToKnownObjects(ObjectIdentifier identifier, IContainerElementNotifyPropertyChanged target)
        {
            _knownObjects.Add(identifier, target);
        }

        private void RemoveFromKnownObjects(ObjectIdentifier identifier, IContainerElementNotifyPropertyChanged target)
        {
            _knownObjects.Remove(identifier);
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