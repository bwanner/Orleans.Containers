﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Orleans.Runtime;
using Orleans.Streams.Endpoints;
using Orleans.Streams.Linq;
using Orleans.Streams.Messages;
using Orleans.Streams.Test.Helpers;
using Orleans.TestingHost;

namespace Orleans.Streams.Test
{
    [TestClass]
    public class LinqTest : TestingSiloHost
    {
        private const string StreamProviderString = "CollectionStreamProvider";
        private IStreamProvider _provider;
        private DefaultStreamProcessorAggregateFactory _factory;


        [TestInitialize]
        public void TestInitialize()
        {
            _provider = GrainClient.GetStreamProvider(StreamProviderString);
            _factory = new DefaultStreamProcessorAggregateFactory(GrainFactory);
        }

        [ClassCleanup]
        public static void ClassCleanup()
        {
            // Optional. 
            // By default, the next test class which uses TestignSiloHost will
            // cause a fresh Orleans silo environment to be created.
            StopAllSilosIfRunning();
        }

        #region Select

        [TestMethod]
        public async Task TestOneLevelSelectDataPass()
        {
            var input = new List<int>() {5, 213, 23, -21, 23, 99}.BatchIEnumerable(2).ToList();
            await
                TestMultiLevelDataPass<int, int>(
                    async (streamSource, factory) => await streamSource.Select(_ => _, factory), input, input,
                    CollectionAssert.AreEquivalent);
        }

        [TestMethod]
        public async Task TestTwoLevelSelect()
        {
            var input = new List<int>() {5, 213, 23, -21, 23, 99}.BatchIEnumerable(2).ToList();

            await
                TestMultiLevelDataPass<int, int>(
                    async (streamSource, factory) => await streamSource.Select(x => x.ToString(), factory).Select(s => int.Parse(s)), 
                    input, input, CollectionAssert.AreEquivalent);
        }

        #endregion

        #region SelectMany

        [TestMethod]
        public async Task TestOneLevelSelectManyDataPass()
        {
            var inputChunks = new List<int>() { 5, 213, 23, -21, 23, 99 }.BatchIEnumerable(2).ToList();
            var outputChunks = inputChunks.SelectMany(i => i).Select(i => (i > 0) ? Enumerable.Range(0, i).ToList() : i.SingleValueToList()).ToList();

            var source = new StreamMessageSenderComposite<int>(_provider, 2);
            var factory = new DefaultStreamProcessorAggregateFactory(GrainFactory);
            var query = await source.SimpleSelectMany(i => (i > 0) ? Enumerable.Range(0, i) : i.SingleValueToList(), factory);

            var queryOutputStreams = await query.GetOutputStreams();

            var resultConsumer = new TestTransactionalTransactionalStreamConsumerAggregate<int>(_provider);
            await resultConsumer.SetInput(queryOutputStreams);

            Assert.AreEqual(2, queryOutputStreams.Count);
            Assert.AreEqual(0, resultConsumer.Items.Count);

            for (int i = 0; i < inputChunks.Count; i++)
            {
                var input = inputChunks[i];
                var expectedOutput = new List<int>();
                expectedOutput.AddRange(outputChunks[2*i]);
                expectedOutput.AddRange(outputChunks[2*i+1]);

                var tid = TransactionGenerator.GenerateTransactionId();
                await source.StartTransaction(tid);
                await source.SendMessage(new ItemMessage<int>(input));
                await source.EndTransaction(tid);

                CollectionAssert.AreEquivalent(expectedOutput, resultConsumer.Items);
                resultConsumer.Items.Clear();
            }

            await query.TearDown();
            await resultConsumer.TearDown();

        }

        #endregion

        #region Where

        [TestMethod]
        public async Task TestOneLevelWhereDataPass()
        {
            var input = new List<int>() { 5, 213, 23, -21, 23, 99 }.BatchIEnumerable(2).ToList();
            var output = new List<List<int>>() {new List<int>() {213}, new List<int>() {23}, new List<int>() {23, 99}};
            await
                TestMultiLevelDataPass<int, int>(
                    async (streamSource, factory) => await streamSource.Where(x => x >= 20, factory), input, output,
                    CollectionAssert.AreEquivalent);
        }

        [TestMethod]
        public async Task TestTwoLevelWhere()
        {
            var input = new List<int>() { 5, 213, 23, -21, 23, 99 };
            var output = new List<string>() {  "213", "23", "23", "99" };

            var source = new StreamMessageSenderComposite<int>(_provider, 2);

            var query = await source.Where(x => x >= 20, _factory).Select(x => x.ToString());
            var queryOutputStreams = await query.GetOutputStreams();

            var resultConsumer = new TransactionalStreamListConsumer<string>(_provider);
            await resultConsumer.SetInput(queryOutputStreams);

            await source.SendMessage(new ItemMessage<int>(input));

            await query.TearDown();
            await resultConsumer.TearDown();
        }

        #endregion

        #region SQO-independent

        [TestMethod]
        public async Task TestTwoLevelAggregateSelectSetupAndTearDown()
        {
            await ValidateTwoLevelAggregateSetupAndTearDown();
        }

        [TestMethod]
        public async Task TestTearDownStreamBroken()
        {
            var source = new StreamMessageSenderComposite<int>(_provider, 2);

            var factory = new DefaultStreamProcessorAggregateFactory(GrainFactory);
            var query = await source.Select(x => x, factory);
            var queryOutputStreams = await query.GetOutputStreams();

            var resultConsumer = new TestTransactionalTransactionalStreamConsumerAggregate<int>(_provider);
            await resultConsumer.SetInput(queryOutputStreams);

            Assert.AreEqual(2, queryOutputStreams.Count);
            Assert.AreEqual(0, resultConsumer.Items.Count);

            Assert.IsFalse(await resultConsumer.AllConsumersTearDownCalled());

            await query.TearDown();

            await resultConsumer.TearDown();
            Assert.IsTrue(await resultConsumer.AllConsumersTearDownCalled());

            await source.SendMessage(new ItemMessage<int>(new List<int>() {2, 3}));

            Assert.AreEqual(0, resultConsumer.Items.Count);
            Assert.IsTrue(await resultConsumer.AllConsumersTearDownCalled());
        }

        #endregion

        private async Task ValidateTwoLevelAggregateSetupAndTearDown()
        {
            int numberOfStreamsPerLevel = 2;


            var source = new StreamMessageSenderComposite<int>(_provider, numberOfStreamsPerLevel);

            var factory = new DefaultStreamProcessorAggregateFactory(GrainFactory);
            var aggregateOne = await factory.CreateSelect<int, int>(_ => _, new StreamProcessorAggregateConfiguration(await source.GetOutputStreams()));
            var aggregateTwo = await factory.CreateSelect<int, int>(_ => _, new StreamProcessorAggregateConfiguration(await aggregateOne.GetOutputStreams()));
            

            var firstElement = new StreamProcessorChainStart<int, int, DefaultStreamProcessorAggregateFactory>(aggregateOne, source, new DefaultStreamProcessorAggregateFactory(GrainFactory));
            var query = new StreamProcessorChain<int, int, DefaultStreamProcessorAggregateFactory>(aggregateTwo, firstElement);

            Assert.IsFalse(await aggregateOne.IsTearedDown());
            Assert.IsFalse(await aggregateTwo.IsTearedDown());

            await query.TearDown();

            Assert.IsTrue(await aggregateOne.IsTearedDown());
            Assert.IsTrue(await aggregateTwo.IsTearedDown());
        }

        private async Task TestMultiLevelDataPass<TIn, TOut>(
            Func<StreamMessageSenderComposite<TIn>, DefaultStreamProcessorAggregateFactory, Task<IStreamProcessorChain<TOut, DefaultStreamProcessorAggregateFactory>>>
                createStreamProcessingChainFunc, List<List<TIn>> inputChunks, List<List<TOut>> outputChunks,
            Action<List<TOut>, List<TOut>> resultAssertion)
        {
            if (inputChunks.Count != outputChunks.Count)
            {
                throw new ArgumentException();
            }

            var source = new StreamMessageSenderComposite<TIn>(_provider, 2);

            var query = await createStreamProcessingChainFunc(source, _factory);
            var queryOutputStreams = await query.GetOutputStreams();

            var resultConsumer = new TestTransactionalTransactionalStreamConsumerAggregate<TOut>(_provider);
            await resultConsumer.SetInput(queryOutputStreams);

            Assert.AreEqual(2, queryOutputStreams.Count);
            Assert.AreEqual(0, resultConsumer.Items.Count);

            for (int i = 0; i < inputChunks.Count; i++)
            {
                var input = inputChunks[i];
                var expectedOutput = outputChunks[i];

                var tid = TransactionGenerator.GenerateTransactionId();
                await source.StartTransaction(tid);
                await source.SendMessage(new ItemMessage<TIn>(input));
                await source.EndTransaction(tid);

                resultAssertion(expectedOutput, resultConsumer.Items);
                resultConsumer.Items.Clear();
            }

            await query.TearDown();
            await resultConsumer.TearDown();
        }
    }
}