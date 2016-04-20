using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using Orleans.Collections.Messages;
using Orleans.Streams;
using Orleans.Streams.Messages;

namespace Orleans.Collections.Observable
{
    public class ObservableContainerNodeGrain<T> : ContainerNodeGrain<T>, IObservableContainerNodeGrain<T>
    {
        private IncomingChangeProcessor _propertyChangedProcessor;

        public override async Task OnActivateAsync()
        {
            await base.OnActivateAsync();
            _propertyChangedProcessor = new IncomingChangeProcessor();
            // TODO re add with outgoing processor _propertyChangedProcessor.ContainerPropertyChanged +=
                //change => OutputProducer.EnqueueMessage(new ItemPropertyChangedMessage(change)); 

            StreamMessageDispatchReceiver.Register<ItemAddMessage<T>>(_propertyChangedProcessor.ProcessItemAddMessage);
            StreamMessageDispatchReceiver.Register<ItemRemoveMessage<T>>(_propertyChangedProcessor.ProcessItemRemoveMessage);
            StreamMessageDispatchReceiver.Register<ItemPropertyChangedMessage>(_propertyChangedProcessor.ProcessItemPropertyChangedMessage);
        }

        public override async Task<IReadOnlyCollection<ContainerElementReference<T>>> AddRange(IEnumerable<T> items)
        {
            var elementReferences = await base.AddRange(items);
            _propertyChangedProcessor.AddItems<T>(items, null);
            var containerElements = elementReferences.Select(r => Elements[r]).ToList();

            await OutputProducer.SendAddItems(containerElements);
            return elementReferences;
        }

        public override async Task<bool> Remove(ContainerElementReference<T> reference)
        {
            var removedItem = Elements[reference];
            await Elements.Remove(reference);

            await OutputProducer.SendRemoveItems(removedItem.ToIEnumerable());

            return true;
        }

        public override async Task Clear()
        {
            var removedItems = Elements.ToList();
            await base.Clear();

            await OutputProducer.SendRemoveItems(removedItems);
        }
    }
}