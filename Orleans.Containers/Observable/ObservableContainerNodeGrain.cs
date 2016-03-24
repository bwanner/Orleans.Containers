using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;

namespace Orleans.Collections.Observable
{
    public class ObservableContainerNodeGrain<T> : ContainerNodeGrain<T>, IObservableContainerNodeGrain<T>
    {
        public override async Task OnActivateAsync()
        {
            await base.OnActivateAsync();
        }

        public override async Task<IReadOnlyCollection<ContainerElementReference<T>>> AddRange(IEnumerable<T> items)
        {
            var elementReferences = await base.AddRange(items);
            var containerElements = elementReferences.Select(r => new ContainerElement<T>(r, List[r])).ToList();

            await StreamProvider.SendItems(containerElements, false);
            return elementReferences;
        }

        public override async Task<bool> Remove(ContainerElementReference<T> reference)
        {
            var item = List[reference];
            await List.Remove(reference);

            var removeArgs = new List<ContainerElement<T>>(1) { new ContainerElement<T>(GetReferenceForItem(reference.Offset, false), item)};
            await StreamProvider.SendItems(removeArgs);

            return true;
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