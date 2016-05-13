using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Threading.Tasks;
using Orleans.Collections;
using Orleans.Streams.Endpoints;
using Orleans.Streams.Stateful.Messages;

namespace Orleans.Streams.Stateful.Endpoints
{
    /// <summary>
    /// Receives remote objects and stores them in a list.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class TransactionalStreamRemoteObjectConsumer<T> : TransactionalStreamConsumer
    {
        protected ILocalReceiveContext ReceiveContext;
        
        /// <summary>
        /// Received items.
        /// </summary>
        public IList<T> Items { get; set; }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="streamProvider">Stream provider to be used.</param>
        public TransactionalStreamRemoteObjectConsumer(IStreamProvider streamProvider, ILocalReceiveContext receiveContext, IList<T> items = null) : base(streamProvider)
        {
            Items = items ?? new List<T>();
            ReceiveContext = receiveContext;
        }

        protected override void SetupMessageDispatcher(StreamMessageDispatchReceiver dispatcher)
        {
            base.SetupMessageDispatcher(dispatcher);
            dispatcher.Register<RemoteItemAddMessage<T>>(ProcessModelItemAddMessage);
            dispatcher.Register<RemoteItemRemoveMessage<T>>(ProcessModelItemRemoveMessage);
            dispatcher.Register<RemoteCollectionChangedMessage>(ProcessModelCollectionChangedMessage);
            dispatcher.Register<RemotePropertyChangedMessage>(ProcessModelPropertyChangedMessage);
        }

        private Task ProcessModelItemAddMessage(RemoteItemAddMessage<T> message)
        {
            foreach (var item in message.Items)
            {
                var resultItem = item.Retrieve(ReceiveContext, LocalContextAction.LookupInsertIfNotFound);
                Items.Add(resultItem);
            }

            return TaskDone.Done;
        }

        private Task ProcessModelItemRemoveMessage(RemoteItemRemoveMessage<T> message)
        {
            foreach (var item in message.Items)
            {
                var resultItem = item.Retrieve(ReceiveContext, LocalContextAction.Delete);
                Items.Remove(resultItem);
            }

            return TaskDone.Done;
        }

        private Task ProcessModelPropertyChangedMessage(RemotePropertyChangedMessage message)
        {
            var sourceItem = message.ElementAffected.Retrieve(ReceiveContext, LocalContextAction.LookupInsertIfNotFound);
            var newValue = message.Value.Retrieve(ReceiveContext, LocalContextAction.LookupInsertIfNotFound);
            var oldValue = message.Value.Retrieve(ReceiveContext, LocalContextAction.Delete); // Remove old value from lookup

            sourceItem.GetType().GetProperty(message.PropertyName).GetSetMethod(true).Invoke(sourceItem, new[] { newValue });

            return TaskDone.Done;
        }

        private Task ProcessModelCollectionChangedMessage(RemoteCollectionChangedMessage message)
        {
            var sourceItem = message.ElementAffected.Retrieve(ReceiveContext, LocalContextAction.LookupInsertIfNotFound);
            var sourceList = (dynamic) sourceItem;

            switch (message.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (var itemToAdd in message.Elements)
                    {
                        sourceList.Add(itemToAdd.Retrieve(ReceiveContext, LocalContextAction.LookupInsertIfNotFound));
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    foreach (var itemToRemove in message.Elements)
                    {
                        sourceList.Remove(itemToRemove.Retrieve(ReceiveContext, LocalContextAction.Delete));
                    }
                    break;
                case NotifyCollectionChangedAction.Reset:
                    sourceList.Clear();
                    break;
                default:
                    throw new NotImplementedException();
            }

            return TaskDone.Done;
        }
    }
}