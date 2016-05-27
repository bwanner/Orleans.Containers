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
        /// <param name="nodeStreamPair"></param>
        /// <returns></returns>
        protected override async Task<IStreamProcessorSelectNodeGrain<TIn, TOut>> InitializeNode(Tuple<IStreamProcessorSelectNodeGrain<TIn, TOut>, StreamIdentity> nodeStreamPair)
        {
            var node = nodeStreamPair.Item1;
            await node.SetFunction(_functionTemplate);
            await node.SubscribeToStreams(nodeStreamPair.Item2.SingleValueToList());

            return node;
        }
    }
}