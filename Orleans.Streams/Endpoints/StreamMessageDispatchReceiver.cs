using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Orleans.Runtime;
using Orleans.Streams.Messages;

namespace Orleans.Streams.Endpoints
{
    /// <summary>
    ///     Subscribe and dispatch IStreamMessage to processors.
    /// </summary>
    public class StreamMessageDispatchReceiver : IStreamMessageVisitor, ITransactionalStreamTearDown
    {
        private readonly Dictionary<Type, List<dynamic>> _callbacks = new Dictionary<Type, List<dynamic>>();
        private readonly IStreamProvider _streamProvider;
        private readonly Func<Task> _tearDownFunc;
        private readonly Logger _logger;
        private List<StreamSubscriptionHandle<IStreamMessage>> _streamHandles;
        private bool _tearDownExecuted;

        /// <summary>
        /// Executed after each message that was succesfully dispatched to a registered function. 
        /// </summary>
        public Func<Task> PostProcessedMessageFunc;

        /// <summary>
        ///     Number of streams subscribed to.
        /// </summary>
        public int SubscriptionCount => _streamHandles.Count;

        /// <summary>
        ///     Constructor.
        /// </summary>
        /// <param name="streamProvider">Stream provider to use.</param>
        /// <param name="logger">Logger.</param>
        /// <param name="tearDownFunc">Function to be executed after tear down.</param>
        public StreamMessageDispatchReceiver(IStreamProvider streamProvider, Logger logger = null, Func<Task> tearDownFunc = null)
        {
            _streamProvider = streamProvider;
            _logger = logger;
            _tearDownFunc = tearDownFunc;
            _tearDownExecuted = false;
            _streamHandles = new List<StreamSubscriptionHandle<IStreamMessage>>();
        }

        /// <summary>
        ///     Process and dispatch a message to registered functions for this message type.
        /// </summary>
        /// <param name="streamMessage"></param>
        /// <returns></returns>
        public async Task Visit(IStreamMessage streamMessage)
        {
            CombinedMessage combinedMessage = streamMessage as CombinedMessage;;
            if (combinedMessage != null)
            {
                foreach (var message in combinedMessage.Messages)
                {
                    await Visit(message);
                }

                return;
            }

            List<dynamic> funcList;
            _callbacks.TryGetValue(streamMessage.GetType(), out funcList);

            if (funcList != null)
            {
                foreach (var func in funcList)
                {
                    if (_logger != null && _logger.IsInfo)
                        _logger.Info("Dispatching message of type {0}", streamMessage.GetType().FullName);
                    await func(streamMessage as dynamic);

                    if (PostProcessedMessageFunc != null)
                    {
                        await PostProcessedMessageFunc();
                    }
                }
            }
        }

        /// <summary>
        ///     Checks if this entity is teared down.
        /// </summary>
        /// <returns></returns>
        public Task<bool> IsTearedDown()
        {
            return Task.FromResult(_tearDownExecuted);
        }

        /// <summary>
        ///     Unsubscribes from all streams this entity consumes and remove references to stream this entity produces.
        /// </summary>
        /// <returns></returns>
        public async Task TearDown()
        {
            if (!_tearDownExecuted)
            {
                _tearDownExecuted = true;
                if (_streamHandles != null)
                {
                    await Task.WhenAll(_streamHandles.Select(s => s.UnsubscribeAsync()));
                    _streamHandles.Clear();
                    _streamHandles = null;
                }

                if (_tearDownFunc != null)
                {
                    await _tearDownFunc();
                }
            }
        }

        /// <summary>
        ///     Subscribe to a stream.
        /// </summary>
        /// <param name="streamIdentity">Stream to subscribe to.</param>
        /// <returns></returns>
        public async Task Subscribe(StreamIdentity streamIdentity)
        {
            _tearDownExecuted = false;
            var messageStream = _streamProvider.GetStream<IStreamMessage>(streamIdentity.Guid, streamIdentity.Namespace);

            _streamHandles.Add(await messageStream.SubscribeAsync(async (message, token) => await Visit(message), async () => await TearDown()));
        }

        /// <summary>
        ///     Register a function to be invoked for a particular type of IStreamMessage. If multiple functions are registered for
        ///     the same type, they are invoked one after another synchronously.
        /// </summary>
        /// <typeparam name="T">Type for which the function has to be invoked.</typeparam>
        /// <param name="func">Function to invoke.</param>
        public void Register<T>(Func<T, Task> func)
        {
            var type = typeof(T);
            if (!_callbacks.ContainsKey(type))
            {
                _callbacks.Add(type, new List<dynamic>());
            }

            _callbacks[type].Add(func);
        }
    }
}