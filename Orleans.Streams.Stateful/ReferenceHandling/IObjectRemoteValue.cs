using System;
using Orleans.Collections;

namespace Orleans.Streams.Stateful
{

    /// <summary>
    /// Manages a local object that is linked to a remote object via an identifier.
    /// </summary>
    public interface IObjectRemoteValue<T> : IObjectRemoteValue
    {
        /// <summary>
        /// Retrieve the local object.
        /// </summary>
        /// <param name="receiveContext">Context to find the local object by an identifier.</param>
        /// <param name="localContextAction">How to modify the resolveContext once the object was retrieved.</param>
        /// <returns>Local object.</returns>
        new T Retrieve(ILocalReceiveContext receiveContext, LocalContextAction localContextAction);
    }

    /// <summary>
    /// Manages a local object that is linked to a remote object via an identifier.
    /// </summary>
    public interface IObjectRemoteValue
    {
        /// <summary>
        /// Retrieve the local object.
        /// </summary>
        /// <param name="receiveContext">Context to find the local object by an identifier.</param>
        /// <param name="localContextAction">How to modify the receiveContext once the object was retrieved.</param>
        /// <returns>Local object.</returns>
        object Retrieve(ILocalReceiveContext receiveContext, LocalContextAction localContextAction);

        /// <summary>
        /// Can be used to compare objects within a grain scope.
        /// </summary>
        object ReferenceComparable { get; }

        /// <summary>
        /// Can be used to identify objects in a global scope.
        /// </summary>
        Guid GlobalIdentifier { get; }
    }

    /// <summary>
    /// Describes what happens to an ILocalReceiveContext to retrieve an object.
    /// </summary>
    public enum LocalContextAction
    {
        /// <summary>
        /// Try to lookup an object and remember its mapping if not already present.
        /// </summary>
        LookupInsertIfNotFound,

        /// <summary>
        /// Lookup an object and delete its mapping if present.
        /// </summary>
        Delete
    }
}