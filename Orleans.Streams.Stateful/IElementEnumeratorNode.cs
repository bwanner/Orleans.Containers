using System;
using System.Threading.Tasks;

namespace Orleans.Streams.Stateful
{
    /// <summary>
    /// The type implementing this interface can write items of type T to consumers.
    /// </summary>
    /// <typeparam name="T">Type of the objects that are written.</typeparam>
    public interface IElementEnumeratorNode<T>
    {
        /// <summary>
        /// Enumerate the contents to a stream.
        /// </summary>
        /// <param name="streamIdentity">Stream to enumerate to.</param>
        /// <param name="transactionId"></param>
        /// <returns></returns>
        Task<Guid> EnumerateToStream(StreamIdentity streamIdentity, Guid transactionId);

        /// <summary>
        /// Enumerate a container to an item.
        /// </summary>
        /// <param name="transactionId"></param>
        /// <returns></returns>
        Task<Guid> EnumerateToSubscribers(Guid? transactionId = null);

    }
}