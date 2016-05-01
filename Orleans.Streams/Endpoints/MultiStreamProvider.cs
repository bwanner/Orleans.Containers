using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Orleans.Runtime;

namespace Orleans.Streams.Endpoints
{
    /// <summary>
    /// Provides multiple transactional streams.
    /// </summary>
    /// <typeparam name="T">Type of items to stream.</typeparam>
    public class MultiStreamProvider<T> : ITransactionalStreamProvider<T>
    {
        private readonly StreamMessageSenderComposite<T> _compositeSender;

        private bool _tearDownExecuted;

        public MultiStreamProvider(IStreamProvider provider, int numberOutputStreams)
        {
            _compositeSender = new StreamMessageSenderComposite<T>(provider, numberOutputStreams);
            _tearDownExecuted = false;
        }

        public Task<IList<StreamIdentity>> GetOutputStreams()
        {
            return _compositeSender.GetOutputStreams();
        }

        public Task<bool> IsTearedDown()
        {
            return Task.FromResult(_tearDownExecuted);
        }

        public async Task TearDown()
        {
            await _compositeSender.TearDown();
            _tearDownExecuted = true;
        }

        /// <summary>
        /// Send items to output streams as one transaction. Items are evenly distributed across providers.
        /// </summary>
        /// <param name="data">Items to send.</param>
        /// <returns>Transaction identifier.</returns>
        public async Task<Guid> SendItems(ICollection<T> data)
        {
            var transactionId = Guid.NewGuid();

            await _compositeSender.StartTransaction(transactionId);
            await _compositeSender.SendItems(data);
            await _compositeSender.EndTransaction(transactionId);

            return transactionId;
        }
    }
}