using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Orleans.Streams.Linq
{
    public static class ExtensionMethods
    {
        #region Select

        public static async Task<StreamProcessorChain<TIn, TOut>> Select<TIn, TOut>(
            this ITransactionalStreamProviderAggregate<TIn> source, Func<TIn, TOut> selectionFunc, IGrainFactory factory)
        {
            return await Select(source, selectionFunc, new DefaultStreamProcessorAggregateFactory(factory));
        }

        public static async Task<StreamProcessorChain<TIn, TOut>> Select<TIn, TOut>(
            this ITransactionalStreamProviderAggregate<TIn> source, Func<TIn, TOut> selectionFunc,
            IStreamProcessorAggregateFactory factory)
        {
            var processorAggregate = await factory.CreateSelect<TIn, TOut>(selectionFunc, await source.GetStreamIdentities());
            var processorChain = new StreamProcessorChainStart<TIn, TOut>(processorAggregate, source, factory);

            return processorChain;
        }

        public static async Task<StreamProcessorChain<TIn, TOut>> Select<TOldIn, TIn, TOut>(
            this StreamProcessorChain<TOldIn, TIn> previousNode, Func<TIn, TOut> selectionFunc)
        {
            var processorAggregate =
                await previousNode.Factory.CreateSelect<TIn, TOut>(selectionFunc, await previousNode.Aggregate.GetStreamIdentities());
            var processorChain = new StreamProcessorChain<TIn, TOut>(processorAggregate, previousNode);

            return processorChain;
        }

        public static async Task<StreamProcessorChain<TIn, TOut>> Select<TOldIn, TIn, TOut>(
            this Task<StreamProcessorChain<TOldIn, TIn>> previousNodeTask, Func<TIn, TOut> selectionFunc)
        {
            var previousNode = await previousNodeTask;
            return await Select(previousNode, selectionFunc);
        }

        #endregion

        #region Where

        public static async Task<StreamProcessorChain<TIn, TIn>> Where<TIn>(
            this ITransactionalStreamProviderAggregate<TIn> source, Func<TIn, bool> filterFunc, IGrainFactory factory)
        {
            return await Where(source, filterFunc, new DefaultStreamProcessorAggregateFactory(factory));
        }

        public static async Task<StreamProcessorChain<TIn, TIn>> Where<TIn>(this ITransactionalStreamProviderAggregate<TIn> source, Func<TIn, bool> filterFunc,
            IStreamProcessorAggregateFactory factory)
        {
            var processorAggregate = await factory.CreateWhere(filterFunc, await source.GetStreamIdentities());
            var processorChain = new StreamProcessorChainStart<TIn, TIn>(processorAggregate, source, factory);

            return processorChain;
        }

        public static async Task<StreamProcessorChain<TIn, TIn>> Where<TOldIn, TIn>(
            this StreamProcessorChain<TOldIn, TIn> previousNode, Func<TIn, bool> filterFunc)
        {
            var processorAggregate = await previousNode.Factory.CreateWhere(filterFunc, await previousNode.Aggregate.GetStreamIdentities());
            var processorChain = new StreamProcessorChain<TIn, TIn>(processorAggregate, previousNode);

            return processorChain;
        }

        public static async Task<StreamProcessorChain<TIn, TIn>> Where<TOldIn, TIn>(
            this Task<StreamProcessorChain<TOldIn, TIn>> previousNodeTask, Func<TIn, bool> filterFunc)
        {
            var previousNode = await previousNodeTask;
            return await Where(previousNode, filterFunc);
        }

        #endregion
    }
}