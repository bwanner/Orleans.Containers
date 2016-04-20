using System;
using System.Collections.Generic;
using Orleans.Collections.Observable;

namespace Orleans.Collections.ObjectState
{
    public class ObjectReferenceCounter
    {
        private readonly Dictionary<ObjectIdentifier, TaggedValue<object>> _knownObjects = new Dictionary<ObjectIdentifier, TaggedValue<object>>();

        public object this[ObjectIdentifier identifier] => _knownObjects[identifier].Value;

        public int Count => _knownObjects.Count;

        public event EventHandler<object> OnObjectAdded;
        public event EventHandler<object> OnObjectRemoved;

        public bool ObjectKnown(ObjectIdentifier identifier)
        {
            return identifier != null && _knownObjects.ContainsKey(identifier);
        }

        /// <summary>
        ///     Increases the reference counter by one.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public void IncreaseReferenceCounter(object obj)
        {
            var objectId = ObjectIdentityGenerator.Instance.GetId(obj);
            if (ObjectKnown(objectId))
            {
                _knownObjects[objectId].Tag += 1;
            }

            _knownObjects[objectId] = new TaggedValue<object>(1, obj);
            OnObjectAdded?.Invoke(this, obj);
        }

        public void AddNewItem(object obj, ObjectIdentifier identifier = null)
        {
            if (identifier != null)
            {
                ObjectIdentityGenerator.Instance.SetId(identifier, obj);
            }
            IncreaseReferenceCounter(obj);
        }

        public bool DecreaseReferenceCounter(object obj)
        {
            var objectId = ObjectIdentityGenerator.Instance.GetId(obj);
            var newValue = _knownObjects[objectId].Tag - 1;
            _knownObjects[objectId].Tag = newValue;

            if (newValue < 1)
            {
                var item = _knownObjects[objectId].Value;
                _knownObjects.Remove(objectId);
                OnObjectRemoved?.Invoke(this, item);
                return true;
            }

            return false;
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