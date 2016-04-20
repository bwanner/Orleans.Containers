using System;
using System.ComponentModel;
using Orleans.Collections.ObjectState;

namespace Orleans.Collections.Observable
{
    public interface IContainerElementNotifyPropertyChanged : INotifyPropertyChanged
    {
    }

    public delegate void ContainerElementPropertyChangedEventHandler(ContainerElementPropertyChangedEventArgs change);

    public static class ContainerElementNotifyPropertyChangedExtension
    {
        public static object ApplyChange(this IContainerElementNotifyPropertyChanged element, string propertyName, object value)
        {
            var oldValue = element.GetType().GetProperty(propertyName).GetGetMethod(true).Invoke(element, null);
            var setter = element.GetType().GetProperty(propertyName).GetSetMethod(true);
            setter.Invoke(element, new object[] {value});

            return oldValue;
        }

        public static ObjectIdentifier GetIdentifier(this IContainerElementNotifyPropertyChanged element)
        {
            return ObjectIdentityGenerator.Instance.GetId(element);
        }
    }
}