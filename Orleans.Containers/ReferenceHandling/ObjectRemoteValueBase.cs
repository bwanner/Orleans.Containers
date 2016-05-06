using System;

namespace Orleans.Collections
{
    public abstract class ObjectRemoteValueBase<T> : IObjectRemoteValue<T>
    {

        object IObjectRemoteValue.Retrieve(ILocalReceiveContext resolveContext, ReceiveAction receiveAction)
        {
            return Retrieve(resolveContext, receiveAction);
        }

        public T Retrieve(ILocalReceiveContext resolveContext, ReceiveAction receiveAction)
        {
            // TODO add support for multiple instances of the same object based on Guid.
            object itemFound = null;
            if (resolveContext.GuidToLocalObjects.ContainsKey(GlobalIdentifier))
                itemFound = resolveContext.GuidToLocalObjects[GlobalIdentifier];

            if (receiveAction == ReceiveAction.Lookup && itemFound == null)
                return default(T);

            else if (receiveAction == ReceiveAction.Insert)
            {
                itemFound = CreateLocalObject(resolveContext, receiveAction);
                if (itemFound != null)
                {
                    resolveContext.GuidToLocalObjects[GlobalIdentifier] = itemFound;
                }
            }

            else if (receiveAction == ReceiveAction.Delete)
            {
                resolveContext.GuidToLocalObjects.Remove(GlobalIdentifier);
            }

            return (T) itemFound;
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