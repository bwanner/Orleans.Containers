using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Orleans.Streams.Linq
{
    /// <summary>
    /// Defines SQO extension methods for stream processing.
    /// </summary>
    public static class ExtensionMethods
    {
        #region Select

        public static async Task<StreamProcessorChain<TIn, TOut, TFactory>> Select<TIn, TOut, TFactory>(
            this ITransactionalStreamProvider<TIn> source, Expression<Func<TIn, TOut>> selectionFunc,
            TFactory factory) where TFactory : IStreamProcessorAggregateFactory
        {
            var processorAggregate = await factory.CreateSelect<TIn, TOut>(selectionFunc, await source.GetOutputStreams());
            var processorChain = new StreamProcessorChainStart<TIn, TOut, TFactory>(processorAggregate, source, factory);

            return processorChain;
        }

        public static async Task<StreamProcessorChain<TIn, TOut, TFactory>> Select<TOldIn, TIn, TOut, TFactory>(
            this StreamProcessorChain<TOldIn, TIn, TFactory> previousNode, Expression<Func<TIn, TOut>> selectionFunc) where TFactory : IStreamProcessorAggregateFactory
        {
            var processorAggregate =
                await previousNode.Factory.CreateSelect<TIn, TOut>(selectionFunc, await previousNode.Aggregate.GetOutputStreams());
            var processorChain = new StreamProcessorChain<TIn, TOut, TFactory>(processorAggregate, previousNode);

            return processorChain;
        }

        public static async Task<StreamProcessorChain<TIn, TOut, TFactory>> Select<TOldIn, TIn, TOut, TFactory>(
            this Task<StreamProcessorChain<TOldIn, TIn, TFactory>> previousNodeTask, Expression<Func<TIn, TOut>> selectionFunc) where TFactory : IStreamProcessorAggregateFactory
        {
            var previousNode = await previousNodeTask;
            return await Select(previousNode, selectionFunc);
        }

        #endregion

        #region SimpleSelectMany

        public static async Task<StreamProcessorChain<TIn, TOut, TFactory>> SimpleSelectMany<TIn, TOut, TFactory>(
            this ITransactionalStreamProvider<TIn> source, Expression<Func<TIn, IEnumerable<TOut>>> selectionFunc,
            TFactory factory) where TFactory : IStreamProcessorAggregateFactory
        {
            var processorAggregate = await factory.CreateSimpleSelectMany(selectionFunc, await source.GetOutputStreams());
            var processorChain = new StreamProcessorChainStart<TIn, TOut, TFactory>(processorAggregate, source, factory);

            return processorChain;
        }

        public static async Task<StreamProcessorChain<TIn, TOut, TFactory>> SimpleSelectMany<TOldIn, TIn, TOut, TFactory>(
            this StreamProcessorChain<TOldIn, TIn, TFactory> previousNode, Expression<Func<TIn, IEnumerable<TOut>>> selectionFunc) where TFactory : IStreamProcessorAggregateFactory
        {
            var processorAggregate =
                await previousNode.Factory.CreateSimpleSelectMany(selectionFunc, await previousNode.Aggregate.GetOutputStreams());
            var processorChain = new StreamProcessorChain<TIn, TOut, TFactory>(processorAggregate, previousNode);

            return processorChain;
        }

        public static async Task<StreamProcessorChain<TIn, TOut, TFactory>> SimpleSelectMany<TOldIn, TIn, TOut, TFactory>(
            this Task<StreamProcessorChain<TOldIn, TIn, TFactory>> previousNodeTask, Expression<Func<TIn, IEnumerable<TOut>>> selectionFunc) where TFactory : IStreamProcessorAggregateFactory
        {
            var previousNode = await previousNodeTask;
            return await SimpleSelectMany(previousNode, selectionFunc);
        }

        #endregion

        #region SelectMany

        public static async Task<StreamProcessorChain<TIn, TOut, TFactory>> SelectMany<TIn, TIntermediate, TOut, TFactory>(
    this ITransactionalStreamProvider<TIn> source, Expression<Func<TIn, IEnumerable<TIntermediate>>> collectionSelectorFunc, Expression<Func<TIn, TIntermediate, TOut>> resultSelectorFunc,
    TFactory factory) where TFactory : IStreamProcessorAggregateFactory
        {
            var processorAggregate = await factory.CreateSelectMany(collectionSelectorFunc, resultSelectorFunc , await source.GetOutputStreams());
            var processorChain = new StreamProcessorChainStart<TIn, TOut, TFactory>(processorAggregate, source, factory);

            return processorChain;
        }

        public static async Task<StreamProcessorChain<TIn, TOut, TFactory>> SelectMany<TOldIn, TIn, TIntermediate, TOut, TFactory>(
            this StreamProcessorChain<TOldIn, TIn, TFactory> previousNode, Expression<Func<TIn, IEnumerable<TIntermediate>>> collectionSelectorFunc, Expression<Func<TIn, TIntermediate, TOut>> resultSelectorFunc) where TFactory : IStreamProcessorAggregateFactory
        {
            var processorAggregate =
                await previousNode.Factory.CreateSelectMany(collectionSelectorFunc, resultSelectorFunc, await previousNode.Aggregate.GetOutputStreams());
            var processorChain = new StreamProcessorChain<TIn, TOut, TFactory>(processorAggregate, previousNode);

            return processorChain;
        }

        public static async Task<StreamProcessorChain<TIn, TOut, TFactory>> SelectMany<TOldIn, TIn, TIntermediate, TOut, TFactory>(
            this Task<StreamProcessorChain<TOldIn, TIn, TFactory>> previousNodeTask, Expression<Func<TIn, IEnumerable<TIntermediate>>> collectionSelectorFunc, Expression<Func<TIn, TIntermediate, TOut>> resultSelectorFunc) where TFactory : IStreamProcessorAggregateFactory
        {
            var previousNode = await previousNodeTask;
            return await SelectMany(previousNode, collectionSelectorFunc, resultSelectorFunc);
        }

        #endregion

        #region Where

        public static async Task<StreamProcessorChain<TIn, TIn, TFactory>> Where<TIn, TFactory>(this ITransactionalStreamProvider<TIn> source, Expression<Func<TIn, bool>> filterFunc,
            TFactory factory) where TFactory : IStreamProcessorAggregateFactory
        {
            var processorAggregate = await factory.CreateWhere(filterFunc, await source.GetOutputStreams());
            var processorChain = new StreamProcessorChainStart<TIn, TIn, TFactory>(processorAggregate, source, factory);

            return processorChain;
        }

        public static async Task<StreamProcessorChain<TIn, TIn, TFactory>> Where<TOldIn, TIn, TFactory>(
            this StreamProcessorChain<TOldIn, TIn, TFactory> previousNode, Expression<Func<TIn, bool>> filterFunc) where TFactory : IStreamProcessorAggregateFactory
        {
            var processorAggregate = await previousNode.Factory.CreateWhere(filterFunc, await previousNode.Aggregate.GetOutputStreams());
            var processorChain = new StreamProcessorChain<TIn, TIn, TFactory>(processorAggregate, previousNode);

            return processorChain;
        }

        public static async Task<StreamProcessorChain<TIn, TIn, TFactory>> Where<TOldIn, TIn, TFactory>(
            this Task<StreamProcessorChain<TOldIn, TIn, TFactory>> previousNodeTask, Expression<Func<TIn, bool>> filterFunc) where TFactory : IStreamProcessorAggregateFactory
        {
            var previousNode = await previousNodeTask;
            return await Where(previousNode, filterFunc);
        }

        #endregion
    }
}