using System;

namespace Orleans.Collections
{
    [Serializable]
    public abstract class ObjectRemoteValueBase<T> : IObjectRemoteValue<T>
    {

        object IObjectRemoteValue.Retrieve(ILocalReceiveContext resolveContext, ReceiveAction receiveAction)
        {
            return Retrieve(resolveContext, receiveAction);
        }

        public T Retrieve(ILocalReceiveContext resolveContext, ReceiveAction receiveAction)
        {
            bool itemFound = false;
            T item = default(T);
            if (resolveContext.GuidToLocalObjects.ContainsKey(GlobalIdentifier))
            {
                item = (T) resolveContext.GuidToLocalObjects[GlobalIdentifier];
                itemFound = true;
            }
            switch (receiveAction)
            {
                case ReceiveAction.LookupInsertIfNotFound:
                    if (!itemFound)
                    {
                        item = CreateLocalObject(resolveContext, receiveAction);
                        if (item != null)
                        {
                            resolveContext.GuidToLocalObjects[GlobalIdentifier] = item;
                        }
                    }
                    if (item != null)
                        return item;
                    break;
                case ReceiveAction.Delete:
                    if(!itemFound)
                        item = CreateLocalObject(resolveContext, receiveAction);
                    resolveContext.GuidToLocalObjects.Remove(GlobalIdentifier);
                    return item;
            }
        
            return item;
        }

        protected abstract T CreateLocalObject(ILocalReceiveContext resolveContext, ReceiveAction receiveAction);

        /// <summary>
        /// Can be used to compare objects within a grain scope.
        /// </summary>
        public abstract object ReferenceComparable { get; }

        /// <summary>
        /// Can be used to identify objects in a global scope.
        /// </summary>
        public abstract Guid GlobalIdentifier { get; }
    }
}