using System;
using System.Collections.Specialized;
using System.ComponentModel;
using Orleans.Collections.ObjectState;

namespace Orleans.Collections.Observable
{
    public class OutgoingChangeProcessor
    {
        public event ContainerElementPropertyChangedEventHandler ContainerPropertyChanged;

        //public event ContainerElementCollectionChangedEventHandler ContainerCollectionChanged;






        private void Target_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var objectId = ObjectIdentityGenerator.Instance.GetId(sender);
            var getMethod = sender.GetType().GetProperty(e.PropertyName).GetGetMethod();
            //ContainerPropertyChanged?.Invoke(new ContainerElementPropertyChangedEventArgs(e.PropertyName, getMethod.Invoke(sender, null), objectId));
        }


        private void List_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:

                    break;
                case NotifyCollectionChangedAction.Remove:

                    break;

                case NotifyCollectionChangedAction.Reset:

                    break;
                default:
                    throw new NotImplementedException();
            }
        }
    }
}