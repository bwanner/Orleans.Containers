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
        /// <param name="receiveContext"></param>
        /// <param name="tearDownFunc">Function to be executed after tear down.</param>
        /// <param name="items"></param>
        public TransactionalStreamRemoteObjectConsumer(IStreamProvider streamProvider, ILocalReceiveContext receiveContext, Func<Task> tearDownFunc = null, IList<T> items = null) : base(streamProvider, tearDownFunc)
        {
            Items = items ?? new List<T>();
            ReceiveContext = receiveContext;
        }

        protected override void SetupMessageDispatcher(StreamMessageDispatchReceiver dispatcher)
        {
            base.SetupMessageDispatcher(dispatcher);
            dispatcher.Register<RemoteItemAddMessage<T>>(ProcessModelItemAddMessage);
            dispatcher.Register<RemoteItemRemoveMessage<T>>(ProcessModelItemRemoveMessage);
            dispatcher.Register<RemoteCollectionChangedMessage>(ProcessRemoteCollectionChangedMessage);
            dispatcher.Register<RemotePropertyChangedMessage>(ProcessRemotePropertyChangedMessage);
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

        private Task ProcessRemotePropertyChangedMessage(RemotePropertyChangedMessage message)
        {
            var sourceItem = message.ElementAffected.Retrieve(ReceiveContext, LocalContextAction.LookupInsertIfNotFound);
            var newValue = message.Value.Retrieve(ReceiveContext, LocalContextAction.LookupInsertIfNotFound);
            var oldValue = message.OldValue.Retrieve(ReceiveContext, LocalContextAction.Delete); // Remove old value from lookup

            sourceItem.GetType().GetProperty(message.PropertyName).GetSetMethod(true).Invoke(sourceItem, new[] { newValue });

            return TaskDone.Done;
        }

        private Task ProcessRemoteCollectionChangedMessage(RemoteCollectionChangedMessage message)
        {
            var sourceItem = message.ElementAffected.Retrieve(ReceiveContext, LocalContextAction.LookupInsertIfNotFound);
            if (sourceItem == null)
                throw new NullReferenceException("Matching collection cannot be retrieved");

            if (message.SourceElementPropertyName != null)
                sourceItem = sourceItem.GetType().GetProperty(message.SourceElementPropertyName).GetValue(sourceItem);

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