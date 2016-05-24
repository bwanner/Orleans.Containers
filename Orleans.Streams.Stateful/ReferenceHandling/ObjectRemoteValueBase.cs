using System;
using Orleans.Collections;

namespace Orleans.Streams.Stateful
{
    /// <summary>
    /// Abstract class providing lookup for a remote object.
    /// </summary>
    /// <typeparam name="T">Type of the remote object.</typeparam>
    [Serializable]
    public abstract class ObjectRemoteValueBase<T> : IObjectRemoteValue<T>
    {
        /// <summary>
        /// Retrieve the local object.
        /// </summary>
        /// <param name="receiveContext">Context to find the local object by an identifier.</param>
        /// <param name="localContextAction">How to modify the receiveContext once the object was retrieved.</param>
        /// <returns>Local object.</returns>
        object IObjectRemoteValue.Retrieve(ILocalReceiveContext receiveContext, LocalContextAction localContextAction)
        {
            return Retrieve(receiveContext, localContextAction);
        }

        /// <summary>
        /// Retrieve the local object.
        /// </summary>
        /// <param name="receiveContext">Context to find the local object by an identifier.</param>
        /// <param name="localContextAction">How to modify the resolveContext once the object was retrieved.</param>
        /// <returns>Local object.</returns>
        public T Retrieve(ILocalReceiveContext receiveContext, LocalContextAction localContextAction)
        {
            object output = null;
            bool itemFound = receiveContext.GuidToLocalObjects.TryGetValue(GlobalIdentifier, out output);
            T item = (output != null) ? (T) output : default(T); 

            switch (localContextAction)
            {
                case LocalContextAction.LookupInsertIfNotFound:
                    if (!itemFound)
                    {
                        item = CreateLocalObject(receiveContext, localContextAction);
                        if (item != null)
                        {
                            receiveContext.GuidToLocalObjects[GlobalIdentifier] = item;
                        }
                    }
                    if (item != null)
                        return item;
                    break;
                case LocalContextAction.Delete:
                    if(!itemFound)
                        item = CreateLocalObject(receiveContext, localContextAction);
                    receiveContext.GuidToLocalObjects.Remove(GlobalIdentifier);
                    return item;
            }
        
            return item;
        }

        protected abstract T CreateLocalObject(ILocalReceiveContext resolveContext, LocalContextAction localContextAction);

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