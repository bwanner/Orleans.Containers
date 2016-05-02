using System;
using System.Threading.Tasks;

namespace Orleans.Streams.Linq.Aggregates
{
    /// <summary>
    ///     Supports defining a select function that is executed by using multiple IStreamProcessorSelectNodeGrain.
    /// </summary>
    /// <typeparam name="TIn">Data input type.</typeparam>
    /// <typeparam name="TOut">Data output type.</typeparam>
    public class StreamProcessorSelectAggregate<TIn, TOut> : StreamProcessorAggregate<TIn, TOut, IStreamProcessorSelectNodeGrain<TIn, TOut>>,
        IStreamProcessorSelectAggregate<TIn, TOut>
    {
        private SerializableFunc<TIn, TOut> _functionTemplate;

        /// <summary>
        ///     Sets the selection function.
        /// </summary>
        /// <param name="function"></param>
        /// <returns></returns>
        public Task SetFunction(SerializableFunc<TIn, TOut> function)
        {
            _functionTemplate = function;
            return TaskDone.Done;
        }

        /// <summary>
        ///     Operation to create a IStreamProcessorNodeGrain of type TNode.
        /// </summary>
        /// <param name="identity">Identity to subscribe the node to.</param>
        /// <returns></returns>
        protected override async Task<IStreamProcessorSelectNodeGrain<TIn, TOut>> InitializeNode(StreamIdentity identity)
        {
            var node = GrainFactory.GetGrain<IStreamProcessorSelectNodeGrain<TIn, TOut>>(Guid.NewGuid());
            await node.SetFunction(_functionTemplate);
            await node.SubscribeToStreams(identity.SingleValueToList());

            return node;
        }
    }
}