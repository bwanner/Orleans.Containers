using System;
using System.Collections.Generic;
using Orleans.Collections.Observable;

namespace Orleans.Collections.ObjectState
{
    public class ObjectReferenceCounter<T>
    {
        private readonly Dictionary<ObjectIdentifier, TaggedValue<T>> _knownObjects = new Dictionary<ObjectIdentifier, TaggedValue<T>>();

        public T this[ObjectIdentifier identifier] => _knownObjects[identifier].Value;

        public int Count => _knownObjects.Count;

        public event EventHandler<T> OnObjectAdded;
        public event EventHandler<T> OnObjectRemoved;

        public bool ObjectKnown(ObjectIdentifier identifier)
        {
            return _knownObjects.ContainsKey(identifier);
        }

        /// <summary>
        ///     Increases the reference counter by one.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public void IncreaseReferenceCounter(T obj)
        {
            var objectId = ObjectIdentityGenerator.Instance.GetId(obj);
            if (ObjectKnown(objectId))
            {
                _knownObjects[objectId].Tag += 1;
            }

            _knownObjects[objectId] = new TaggedValue<T>(1, obj);
            OnObjectAdded?.Invoke(this, obj);
        }

        public void AddNewItem(T obj, ObjectIdentifier identifier = null)
        {
            if (identifier != null)
            {
                ObjectIdentityGenerator.Instance.SetId(identifier, obj);
            }
            IncreaseReferenceCounter(obj);
        }

        public void DecreaseReferenceCounter(T obj)
        {
            var objectId = ObjectIdentityGenerator.Instance.GetId(obj);
            var newValue = _knownObjects[objectId].Tag - 1;
            _knownObjects[objectId].Tag = newValue;

            if (newValue < 1)
            {
                var item = _knownObjects[objectId].Value;
                _knownObjects.Remove(objectId);
                OnObjectRemoved?.Invoke(this, item);
            }
        }
    }

    internal class TaggedValue<T>
    {
        public int Tag { get; set; }
        public T Value { get; set; }

        public TaggedValue(int tag, T obj)
        {
            Tag = tag;
            Value = obj;
        }
    }
}