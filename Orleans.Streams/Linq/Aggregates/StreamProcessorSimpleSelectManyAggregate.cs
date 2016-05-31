using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Orleans.Streams.Linq.Aggregates
{
    /// <summary>
    ///     Supports defining a selectMany function that is executed by using multiple IStreamProcessorSimpleSelectManyNodeGrain.
    /// </summary>
    /// <typeparam name="TIn">Data input type.</typeparam>
    /// <typeparam name="TOut">Data output type.</typeparam>
    public class StreamProcessorSimpleSelectManyAggregate<TIn, TOut> : StreamProcessorAggregate<TIn, TOut, IStreamProcessorSimpleSelectManyNodeGrain<TIn, TOut>>,
        IStreamProcessorSimpleSelectManyAggregate<TIn, TOut>
    {
        private SerializableFunc<TIn, IEnumerable<TOut>> _functionTemplate;

        /// <summary>
        ///     Sets the selection function.
        /// </summary>
        /// <param name="function"></param>
        /// <returns></returns>
        public Task SetFunction(SerializableFunc<TIn, IEnumerable<TOut>> function)
        {
            _functionTemplate = function;
            return TaskDone.Done;
        }

        /// <summary>
        ///     Operation to create a IStreamProcessorNodeGrain of type TNode.
        /// </summary>
        /// <param name="nodeStreamPair"></param>
        /// <returns></returns>
        protected override async Task<IStreamProcessorSimpleSelectManyNodeGrain<TIn, TOut>> InitializeNode(Tuple<IStreamProcessorSimpleSelectManyNodeGrain<TIn, TOut>, StreamIdentity> nodeStreamPair)
        {
            var node = nodeStreamPair.Item1;
            await node.SetFunction(_functionTemplate);
            await node.SubscribeToStreams(nodeStreamPair.Item2.SingleValueToList());

            return node;
        }
    }
}