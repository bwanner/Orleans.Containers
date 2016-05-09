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
            TFactory factory, int scatterFactor = 1) where TFactory : IStreamProcessorAggregateFactory
        {
            var aggregateConfiguration = new StreamProcessorAggregateConfiguration(await  source.GetOutputStreams(), scatterFactor);
            var processorAggregate = await factory.CreateSelect<TIn, TOut>(selectionFunc, aggregateConfiguration);
            var processorChain = new StreamProcessorChainStart<TIn, TOut, TFactory>(processorAggregate, source, factory);

            return processorChain;
        }

        public static async Task<StreamProcessorChain<TIn, TOut, TFactory>> Select<TOldIn, TIn, TOut, TFactory>(
            this StreamProcessorChain<TOldIn, TIn, TFactory> previousNode, Expression<Func<TIn, TOut>> selectionFunc, int scatterFactor = 1) where TFactory : IStreamProcessorAggregateFactory
        {
            var aggregateConfiguration = new StreamProcessorAggregateConfiguration(await previousNode.Aggregate.GetOutputStreams(), scatterFactor);
            var processorAggregate =
                await previousNode.Factory.CreateSelect<TIn, TOut>(selectionFunc, aggregateConfiguration);
            var processorChain = new StreamProcessorChain<TIn, TOut, TFactory>(processorAggregate, previousNode);

            return processorChain;
        }

        public static async Task<StreamProcessorChain<TIn, TOut, TFactory>> Select<TOldIn, TIn, TOut, TFactory>(
            this Task<StreamProcessorChain<TOldIn, TIn, TFactory>> previousNodeTask, Expression<Func<TIn, TOut>> selectionFunc, int scatterFactor = 1) where TFactory : IStreamProcessorAggregateFactory
        {
            var previousNode = await previousNodeTask;
            return await Select(previousNode, selectionFunc);
        }

        #endregion

        #region SimpleSelectMany

        public static async Task<StreamProcessorChain<TIn, TOut, TFactory>> SimpleSelectMany<TIn, TOut, TFactory>(
            this ITransactionalStreamProvider<TIn> source, Expression<Func<TIn, IEnumerable<TOut>>> selectionFunc,
            TFactory factory, int scatterFactor = 1) where TFactory : IStreamProcessorAggregateFactory
        {
            var aggregateConfiguration = new StreamProcessorAggregateConfiguration(await source.GetOutputStreams(), scatterFactor);
            var processorAggregate = await factory.CreateSimpleSelectMany(selectionFunc, aggregateConfiguration);
            var processorChain = new StreamProcessorChainStart<TIn, TOut, TFactory>(processorAggregate, source, factory);

            return processorChain;
        }

        public static async Task<StreamProcessorChain<TIn, TOut, TFactory>> SimpleSelectMany<TOldIn, TIn, TOut, TFactory>(
            this StreamProcessorChain<TOldIn, TIn, TFactory> previousNode, Expression<Func<TIn, IEnumerable<TOut>>> selectionFunc, int scatterFactor = 1) where TFactory : IStreamProcessorAggregateFactory
        {
            var aggregateConfiguration = new StreamProcessorAggregateConfiguration(await previousNode.Aggregate.GetOutputStreams(), scatterFactor);
            var processorAggregate =
                await previousNode.Factory.CreateSimpleSelectMany(selectionFunc, aggregateConfiguration);
            var processorChain = new StreamProcessorChain<TIn, TOut, TFactory>(processorAggregate, previousNode);

            return processorChain;
        }

        public static async Task<StreamProcessorChain<TIn, TOut, TFactory>> SimpleSelectMany<TOldIn, TIn, TOut, TFactory>(
            this Task<StreamProcessorChain<TOldIn, TIn, TFactory>> previousNodeTask, Expression<Func<TIn, IEnumerable<TOut>>> selectionFunc, int scatterFactor = 1) where TFactory : IStreamProcessorAggregateFactory
        {
            var previousNode = await previousNodeTask;
            return await SimpleSelectMany(previousNode, selectionFunc);
        }

        #endregion

        #region SelectMany

        public static async Task<StreamProcessorChain<TIn, TOut, TFactory>> SelectMany<TIn, TIntermediate, TOut, TFactory>(
    this ITransactionalStreamProvider<TIn> source, Expression<Func<TIn, IEnumerable<TIntermediate>>> collectionSelectorFunc, Expression<Func<TIn, TIntermediate, TOut>> resultSelectorFunc,
    TFactory factory, int scatterFactor = 1) where TFactory : IStreamProcessorAggregateFactory
        {
            var aggregateConfiguration = new StreamProcessorAggregateConfiguration(await source.GetOutputStreams(), scatterFactor);
            var processorAggregate = await factory.CreateSelectMany(collectionSelectorFunc, resultSelectorFunc , aggregateConfiguration);
            var processorChain = new StreamProcessorChainStart<TIn, TOut, TFactory>(processorAggregate, source, factory);

            return processorChain;
        }

        public static async Task<StreamProcessorChain<TIn, TOut, TFactory>> SelectMany<TOldIn, TIn, TIntermediate, TOut, TFactory>(
            this StreamProcessorChain<TOldIn, TIn, TFactory> previousNode, Expression<Func<TIn, IEnumerable<TIntermediate>>> collectionSelectorFunc, Expression<Func<TIn, TIntermediate, TOut>> resultSelectorFunc, int scatterFactor = 1) where TFactory : IStreamProcessorAggregateFactory
        {
            var aggregateConfiguration = new StreamProcessorAggregateConfiguration(await previousNode.Aggregate.GetOutputStreams(), scatterFactor);
            var processorAggregate =
                await previousNode.Factory.CreateSelectMany(collectionSelectorFunc, resultSelectorFunc, aggregateConfiguration);
            var processorChain = new StreamProcessorChain<TIn, TOut, TFactory>(processorAggregate, previousNode);

            return processorChain;
        }

        public static async Task<StreamProcessorChain<TIn, TOut, TFactory>> SelectMany<TOldIn, TIn, TIntermediate, TOut, TFactory>(
            this Task<StreamProcessorChain<TOldIn, TIn, TFactory>> previousNodeTask, Expression<Func<TIn, IEnumerable<TIntermediate>>> collectionSelectorFunc, Expression<Func<TIn, TIntermediate, TOut>> resultSelectorFunc, int scatterFactor = 1) where TFactory : IStreamProcessorAggregateFactory
        {
            var previousNode = await previousNodeTask;
            return await SelectMany(previousNode, collectionSelectorFunc, resultSelectorFunc);
        }

        #endregion

        #region Where

        public static async Task<StreamProcessorChain<TIn, TIn, TFactory>> Where<TIn, TFactory>(this ITransactionalStreamProvider<TIn> source, Expression<Func<TIn, bool>> filterFunc,
            TFactory factory, int scatterFactor = 1) where TFactory : IStreamProcessorAggregateFactory
        {
            var aggregateConfiguration = new StreamProcessorAggregateConfiguration(await source.GetOutputStreams(), scatterFactor);
            var processorAggregate = await factory.CreateWhere(filterFunc, aggregateConfiguration);
            var processorChain = new StreamProcessorChainStart<TIn, TIn, TFactory>(processorAggregate, source, factory);

            return processorChain;
        }

        public static async Task<StreamProcessorChain<TIn, TIn, TFactory>> Where<TOldIn, TIn, TFactory>(
            this StreamProcessorChain<TOldIn, TIn, TFactory> previousNode, Expression<Func<TIn, bool>> filterFunc, int scatterFactor = 1) where TFactory : IStreamProcessorAggregateFactory
        {
            var aggregateConfiguration = new StreamProcessorAggregateConfiguration(await previousNode.Aggregate.GetOutputStreams(), scatterFactor);
            var processorAggregate = await previousNode.Factory.CreateWhere(filterFunc, aggregateConfiguration);
            var processorChain = new StreamProcessorChain<TIn, TIn, TFactory>(processorAggregate, previousNode);

            return processorChain;
        }

        public static async Task<StreamProcessorChain<TIn, TIn, TFactory>> Where<TOldIn, TIn, TFactory>(
            this Task<StreamProcessorChain<TOldIn, TIn, TFactory>> previousNodeTask, Expression<Func<TIn, bool>> filterFunc, int scatterFactor = 1) where TFactory : IStreamProcessorAggregateFactory
        {
            var previousNode = await previousNodeTask;
            return await Where(previousNode, filterFunc, scatterFactor);
        }

        #endregion
    }
}