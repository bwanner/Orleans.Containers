using System;
using System.Threading.Tasks;
using Orleans.Streams.Partitioning;

namespace Orleans.Streams.Linq.Aggregates
{
    /// <summary>
    ///     Supports defining a where function that is executed by using multiple IStreamProcessorWhereNodeGrain.
    /// </summary>
    /// <typeparam name="TIn">Data input/output type.</typeparam>
    public class StreamProcessorWhereAggregate<TIn> : StreamProcessorAggregate<TIn, TIn, IStreamProcessorWhereNodeGrain<TIn>>,
        IStreamProcessorWhereAggregate<TIn>
    {
        private SerializableFunc<TIn, bool> _functionTemplate;

        /// <summary>
        ///     Define the where function.
        /// </summary>
        /// <param name="function"></param>
        /// <returns></returns>
        public Task SetFunction(SerializableFunc<TIn, bool> function)
        {
            _functionTemplate = function;
            return TaskDone.Done;
        }

        /// <summary>
        ///     Operation to create a IStreamProcessorNodeGrain of type TNode.
        /// </summary>
        /// <param name="nodeStreamPair"></param>
        /// <returns></returns>
        protected override async Task<IStreamProcessorWhereNodeGrain<TIn>> InitializeNode(Tuple<IStreamProcessorWhereNodeGrain<TIn>, StreamIdentity> nodeStreamPair)
        {
            var node = nodeStreamPair.Item1;
            await node.SetFunction(_functionTemplate);
            await node.SubscribeToStreams(nodeStreamPair.Item2.SingleValueToList());

            return node;
        }
    }
}