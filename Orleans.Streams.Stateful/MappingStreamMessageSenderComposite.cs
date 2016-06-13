using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Orleans.Streams.Endpoints;
using Orleans.Streams.Stateful.Messages;

namespace Orleans.Streams.Stateful
{
    /// <summary>
    /// Sends messages via multiple children output. Maintains link between sent objects and a particular stream via reference identity.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class MappingStreamMessageSenderComposite<T> : StreamMessageSenderComposite<T>
    {
        protected ConditionalWeakTable<object, StreamMessageSender<T>> ObjectToSenderMapping;
        private int _currentSenderIndex;

        /// <summary>
        /// Create a new MappingStreamMessageSenderComposite.
        /// </summary>
        /// <param name="provider">Stream provider to use.</param>
        /// <param name="numberOfChildren">Number of sending streams.</param>
        public MappingStreamMessageSenderComposite(IStreamProvider provider, int numberOfChildren = 1) : base(provider, numberOfChildren)
        {
            ObjectToSenderMapping = new ConditionalWeakTable<object, StreamMessageSender<T>>();
            _currentSenderIndex = 0;
        }

        /// <summary>
        /// Send message containing added remote value.
        /// </summary>
        /// <param name="remoteValue">Added remote value.</param>
        public void EnqueueAddRemoteItem(IObjectRemoteValue<T> remoteValue)
        {
            var sender = GetSenderForValue(remoteValue);
            sender.EnqueueMessage(new RemoteItemAddMessage<T>(remoteValue.SingleValueToList()));
        }

        /// <summary>
        /// Send message containing removed remote values.
        /// </summary>
        /// <param name="remoteValues">Removed remote values.</param>
        public void EnqueueRemoveRemoteItems(IList<IObjectRemoteValue<T>> remoteValues)
        {
            var senderGrouping = remoteValues.GroupBy(GetSenderForValue);
            foreach (var group in senderGrouping)
            {
                var senderMessage = new RemoteItemRemoveMessage<T>(group.ToList());
                group.Key.EnqueueMessage(senderMessage);
            }
        }

        private StreamMessageSender<T> GetSenderForValue(IObjectRemoteValue<T> remoteValue)
        {
            StreamMessageSender<T> sender;
            if (ObjectToSenderMapping.TryGetValue(remoteValue.ReferenceComparable, out sender))
                return sender;

            sender = Senders[_currentSenderIndex];
            ObjectToSenderMapping.Add(remoteValue.ReferenceComparable, sender);

            _currentSenderIndex = (_currentSenderIndex + 1) % Senders.Count;

            return sender;
        }
    }
}