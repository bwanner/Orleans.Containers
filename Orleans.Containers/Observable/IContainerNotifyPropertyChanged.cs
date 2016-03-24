namespace Orleans.Collections.Observable
{
    public interface IContainerElementNotifyPropertyChanged
    {
        ObjectIdentifier Identifier { get; }

        event ContainerElementPropertyChangedEventHandler ContainerPropertyChanged;

        void ApplyChange(ContainerElementPropertyChangedEventArgs change);
    }

    public delegate void ContainerElementPropertyChangedEventHandler(ContainerElementPropertyChangedEventArgs change);
}