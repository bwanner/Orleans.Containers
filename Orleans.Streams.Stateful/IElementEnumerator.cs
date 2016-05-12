using System;
using System.Threading.Tasks;
using Orleans.Streams;

namespace Orleans.Collections
{
    public interface IElementEnumerator<T>
    {
        /// <summary>
        /// Enumerate all items to the output stream in batches.
        /// </summary>
        /// <param name="batchSize">Size of one batch.</param>
        /// <returns></returns>
        Task<Guid> EnumerateToSubscribers(int batchSize = int.MaxValue);

        Task<Guid> EnumerateToStream(params StreamIdentity[] streamIdentities);
    }
}