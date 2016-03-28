using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using Orleans.Streams.Messages;

namespace Orleans.Collections.Observable
{
    public class ObservableContainerNodeGrain<T> : ContainerNodeGrain<T>, IObservableContainerNodeGrain<T>
    {
        private DistributedPropertyChangedProcessor<T> _propertyChangedProcessor;

        public override async Task OnActivateAsync()
        {
            await base.OnActivateAsync();
            _propertyChangedProcessor = new DistributedPropertyChangedProcessor<T>();

            StreamMessageDispatchReceiver.Register<ItemMessage<T>>(_propertyChangedProcessor.ProcessItemMessage);
            StreamMessageDispatchReceiver.Register<ItemPropertyChangedMessage>(_propertyChangedProcessor.ProcessItemPropertyChangedMessage);
            // Forward all property changes
            StreamMessageDispatchReceiver.Register<ItemPropertyChangedMessage>(StreamMessageSender.SendMessage);
        }

        public override async Task<IReadOnlyCollection<ContainerElementReference<T>>> AddRange(IEnumerable<T> items)
        {
            var elementReferences = await base.AddRange(items);
            _propertyChangedProcessor.AddItems(items);
            var containerElements = elementReferences.Select(r => new ContainerElement<T>(r, List[r])).ToList();

            await StreamTransactionSender.SendItems(containerElements, false);
            return elementReferences;
        }

        public override async Task<bool> Remove(ContainerElementReference<T> reference)
        {
            var item = List[reference];
            await List.Remove(reference);
            _propertyChangedProcessor.Remove(item);

            var deletedReference = new ContainerElementReference<T>(reference.ContainerId, reference.Offset, null, null, false);
            var removeArgs = new List<ContainerElement<T>>(1) { new ContainerElement<T>(deletedReference, item)};
            await StreamTransactionSender.SendItems(removeArgs);

            return true;
        }

        public override async Task Clear()
        {
            var removedItems = List.ToList();
            foreach (var removedItem in removedItems)
            {
                removedItem.Reference = new ContainerElementReference<T>(removedItem.Reference.ContainerId, removedItem.Reference.Offset, null, null, false);
            }

            await base.Clear();
            await StreamTransactionSender.SendItems(removedItems);
        }

        //private void Foo()
        //{
        //    var t = new Task(async () => await HandleCollectionChange(null, null));
        //    t.RunSynchronously();
        //}

        //private async Task HandleCollectionChange(object sender, NotifyCollectionChangedEventArgs e)
        //{
        //    switch(e.Action)
        //    {
        //        case NotifyCollectionChangedAction.Add:
        //            int newItemsCount = e.NewItems.Count;
        //            var argsAdd = Enumerable.Range(e.NewStartingIndex, newItemsCount).Select(i => new ContainerElement<T>(GetReferenceForItem(i, true), List[i])).ToList();
        //            await StreamProvider.SendItems(argsAdd);
        //        break;
        //        case NotifyCollectionChangedAction.Remove:
        //            int oldItemsCount = e.OldItems.Count;
        //            var argsDel = Enumerable.Range(e.OldStartingIndex, oldItemsCount).Select(i => new ContainerElement<T>(GetReferenceForItem(i, false), List[i])).ToList();
        //            await StreamProvider.SendItems(argsDel);
        //        break;
        //        case NotifyCollectionChangedAction.Reset:
        //            throw new NotImplementedException();
        //        break;
        //    }
        //}
    }
}