namespace Orleans.Collections.Observable
{
    /// <summary>
    /// A container that transmits changes to the list of contained items and
    /// changes within items implementing IContainerNotifyPropertyChanged via an output stream.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IObservableContainerGrain<T> : IContainerGrain<T>
    {
         
    }
}