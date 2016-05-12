using System;

namespace Orleans.Streams.Stateful.Messages
{
    /// <summary>
    /// Message to notify of property changes. Serves as a variant of INotifyPropertyChanged for remote objects.
    /// </summary>
    [Serializable]
    public class RemotePropertyChangedMessage : RemoteObjectStreamMessageBase
    {
        /// <summary>
        /// New value of the changed property.
        /// </summary>
        public IObjectRemoteValue Value { get; private set; }

        /// <summary>
        /// The changed property's name.
        /// </summary>
        public string PropertyName { get; private set; }

        /// <summary>
        /// The old value of the changed property.
        /// </summary>
        public IObjectRemoteValue OldValue { get; set; }

        /// <summary>
        /// Create a new RemotePropertyChangedMessage.
        /// </summary>
        /// <param name="value">New value of the changed property.</param>
        /// <param name="oldValue">Old value of the changed property.</param>
        /// <param name="elementAffected">Oject affected by the property change.</param>
        /// <param name="propertyName">Name of the changed property.</param>
        public RemotePropertyChangedMessage(IObjectRemoteValue value, IObjectRemoteValue oldValue, IObjectRemoteValue elementAffected, string propertyName)
            : base(elementAffected)
        {
            Value = value;
            OldValue = oldValue;
            PropertyName = propertyName;
        }
    }
}