using System;

namespace Orleans.Collections
{
    public interface IObjectRemoteValue<T> : IObjectRemoteValue
    {
        new T Retrieve(ILocalReceiveContext resolveContext, ReceiveAction receiveAction);
    }

    public interface IObjectRemoteValue
    {
        object Retrieve(ILocalReceiveContext resolveContext, ReceiveAction receiveAction);

        /// <summary>
        /// Can be used to compare objects within a grain scope.
        /// </summary>
        object ReferenceComparable { get; }

        /// <summary>
        /// Can be used to identify objects in a global scope.
        /// </summary>
        Guid GlobalIdentifier { get; }
    }

    public enum ReceiveAction
    {
        Lookup,
        Insert,
        Delete
    }
}