namespace Orleans.Collections.Observable
{
    public interface IContainerElementNotifyPropertyChanged
    {
        ObjectIdentifier Identifier { get; }

        event ContainerElementPropertyChangedEventHandler ContainerPropertyChanged;

        void ApplyChange(ContainerElementPropertyChangedEventArgs change);

        /// <summary>
        /// Quick workaround until https://github.com/dotnet/orleans/issues/1645 is fixed.
        /// </summary>
        /// <returns></returns>
        //object DeepCopy(); // TODO
    }

    public delegate void ContainerElementPropertyChangedEventHandler(ContainerElementPropertyChangedEventArgs change);
}