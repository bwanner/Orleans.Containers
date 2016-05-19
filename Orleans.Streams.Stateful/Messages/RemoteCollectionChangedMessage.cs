using System;
using System.Collections.Specialized;

namespace Orleans.Streams.Stateful.Messages
{
    /// <summary>
    ///     Message to notify of collection changes. Serves as a variant of INotifyCollectionChanged for remote objects.
    /// </summary>
    [Serializable]
    public class RemoteCollectionChangedMessage : RemoteObjectStreamMessageBase
    {
        /// <summary>
        ///     Action that was executed on the remote collection.
        /// </summary>
        public NotifyCollectionChangedAction Action { get; private set; }

        /// <summary>
        ///     Remote objects that are affected by the change.
        /// </summary>
        public IObjectRemoteValue[] Elements { get; private set; }

        /// <summary>
        ///     Property name identifying the collection on the source element. If null then sourceElement is the list itself.
        /// </summary>
        public string SourceElementPropertyName { get; set; }

        /// <summary>
        ///     Create a new RemotePropertyChangedMessage.
        /// </summary>
        /// <param name="action">Action that was executed on the remote collection.</param>
        /// <param name="sourceElement">Collection the change occurred on.</param>
        /// <param name="elements">Remote objects that are affected by the change.</param>
        /// <param name="sourceElementPropertyName">
        ///     Property name identifying the collection on the source element. If null then
        ///     sourceElement is the list itself.
        /// </param>
        public RemoteCollectionChangedMessage(NotifyCollectionChangedAction action, IObjectRemoteValue sourceElement, IObjectRemoteValue[] elements,
            string sourceElementPropertyName = null) : base(sourceElement)
        {
            Action = action;
            Elements = elements;
            SourceElementPropertyName = sourceElementPropertyName;
        }
    }
}