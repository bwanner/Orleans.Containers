using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Orleans.Streams.Messages;

namespace Orleans.Streams.Endpoints
{
    /// <summary>
    ///     Allows to send messages via an output stream.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IStreamMessageSender<T> : ITransactionalStreamProvider<T>
    {
        /// <summary>
        ///     Send items via this strem.
        /// </summary>
        /// <param name="items">Items to send.</param>
        /// <returns></returns>
        Task SendItems(IEnumerable<T> items);

        /// <summary>
        ///     Start a transaction.
        /// </summary>
        /// <param name="transactionId">Transaction identifier.</param>
        /// <returns></returns>
        Task StartTransaction(Guid transactionId);

        /// <summary>
        ///     End a transaction.
        /// </summary>
        /// <param name="transactionId">Transaction identifier.</param>
        /// <returns></returns>
        Task EndTransaction(Guid transactionId);

        /// <summary>
        ///     Send a AddItemMessage for all items.
        /// </summary>
        /// <param name="items">Items to send.</param>
        /// <returns></returns>
        Task SendAddItems(IEnumerable<T> items);

        /// <summary>
        ///     Send a UpdateItemMessage for all items.
        /// </summary>
        /// <param name="items">Items to send.</param>
        /// <returns></returns>
        Task SendUpdateItems(IEnumerable<T> items);

        /// <summary>
        ///     Send a RemoveItemMessage for all items.
        /// </summary>
        /// <param name="items">Items to send.</param>
        /// <returns></returns>
        Task SendRemoveItems(IEnumerable<T> items);

        /// <summary>
        ///     Sends a generic message.
        /// </summary>
        /// <param name="message">Message to send.</param>
        /// <returns></returns>
        Task SendMessage(IStreamMessage message);

        /// <summary>
        ///     Enqueue a AddItemMessage for all items.
        /// </summary>
        /// <param name="items">Items to send.</param>
        /// <returns></returns>
        void EnqueueAddItems(IEnumerable<T> items);

        /// <summary>
        ///     Enqueue a UpdateItemMessage for all items.
        /// </summary>
        /// <param name="items">Items to send.</param>
        /// <returns></returns>
        void EnqueueUpdateItems(IEnumerable<T> items);

        /// <summary>
        ///     Enqueue a RemoveItemMessage for all items.
        /// </summary>
        /// <param name="items">Items to send.</param>
        /// <returns></returns>
        void EnqueueRemoveItems(IEnumerable<T> items);

        /// <summary>
        ///     Enqeue a generic message.
        /// </summary>
        /// <param name="message">Message to send.</param>
        /// <returns></returns>
        void EnqueueMessage(IStreamMessage message);

        /// <summary>
        ///     Send all messages and empty the queue.
        /// </summary>
        /// <returns></returns>
        Task FlushQueue();
    }
}