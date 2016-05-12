using System;
using System.Threading.Tasks;
using Orleans.Streams;

namespace Orleans.Collections
{
    /// <summary>
    /// Enumerates items of type T.
    /// </summary>
    /// <typeparam name="T">Type of items to enumerate.</typeparam>
    public interface IElementEnumerator<T>
    {
        /// <summary>
        /// Enumerate all items to the output stream in batches.
        /// </summary>
        /// <param name="batchSize">Size of one batch.</param>
        /// <returns></returns>
        Task<Guid> EnumerateToSubscribers(int batchSize = int.MaxValue);

        /// <summary>
        /// Enumerate the contents to a stream.
        /// </summary>
        /// <param name="streamIdentities">Streams to enumerate to.</param>
        /// <returns></returns>
        Task<Guid> EnumerateToStream(params StreamIdentity[] streamIdentities);
    }
}