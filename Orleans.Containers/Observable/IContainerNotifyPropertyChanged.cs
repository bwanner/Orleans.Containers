namespace Orleans.Collections.Observable
{
    public interface IContainerElementNotifyPropertyChanged
    {
        event ContainerElementPropertyChangedEventHandler PropertyChanged;

        void ApplyChange(ContainerElementPropertyChanged change);
    }

    public delegate void ContainerElementPropertyChangedEventHandler(ContainerElementPropertyChanged change);
}