using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Orleans.Streams;
using Orleans.Streams.Endpoints;
using Orleans.Streams.Linq.Aggregates;
using Orleans.Streams.Messages;
using Orleans.Streams.Test.Helpers;
using Orleans.TestingHost;

namespace Orleans.Collections.Test
{
    [TestClass]
    public class StreamProcessorNodeUnitTest : TestingSiloHost
    {
        private const string StreamProvider = "CollectionStreamProvider";

        [ClassCleanup]
        public static void ClassCleanup()
        {
            // Optional. 
            // By default, the next test class which uses TestignSiloHost will
            // cause a fresh Orleans silo environment to be created.
            StopAllSilosIfRunning();
        }

        [TestMethod]
        public async Task TestTransactionNoItems()
        {
            var processorNodeGuid = Guid.NewGuid();

            var processor = GrainClient.GrainFactory.GetGrain<IStreamProcessorSelectNodeGrain<int, int>>(processorNodeGuid);

            await processor.SetFunction(new SerializableFunc<int, int>(_ => _));

            var provider = GrainClient.GetStreamProvider(StreamProvider);
            var testProvider = new StreamMessageSenderComposite<int>(provider, 1);
            await processor.SubscribeToStreams(await testProvider.GetOutputStreams());

            var testConsumer = new TransactionalStreamListConsumer<int>(provider);

            await SubscribeConsumer(processor, testConsumer);

            await testProvider.SendMessage(new ItemMessage<int>(new List<int>()));
            Assert.AreEqual(0, testConsumer.Items.Count);

            await testProvider.TearDown();
        }

        [TestMethod]
        public async Task TestItemTransfer()
        {
            var processorNodeGuid = Guid.NewGuid();
            var processor = GrainClient.GrainFactory.GetGrain<IStreamProcessorSelectNodeGrain<int, int>>(processorNodeGuid);
            await processor.SetFunction(new SerializableFunc<int, int>(_ => _));

            var itemsToSend = new List<int> {-1, 5, 30};

            var provider = GrainClient.GetStreamProvider(StreamProvider);
            var testProvider = new StreamMessageSenderComposite<int>(provider, 1);
            await processor.SubscribeToStreams(await testProvider.GetOutputStreams());

            var testConsumer = new TransactionalStreamListConsumer<int>(provider);
            await SubscribeConsumer(processor, testConsumer);

            var tid = TransactionGenerator.GenerateTransactionId();
            await testProvider.StartTransaction(tid);
            await testProvider.SendMessage(new ItemMessage<int>(itemsToSend));
            await testProvider.EndTransaction(tid);

            CollectionAssert.AreEquivalent(itemsToSend, testConsumer.Items);

            await testProvider.TearDown();
        }

        private async Task SubscribeConsumer(IStreamProcessorSelectNodeGrain<int, int> processor, TransactionalStreamListConsumer<int> testConsumer)
        {
            await testConsumer.SetInput(await processor.GetOutputStreams());
        }

        [TestMethod]
        public async Task TestItemAggregation()
        {
            var aggregate = GrainClient.GrainFactory.GetGrain<IStreamProcessorSelectAggregate<int, int>>(Guid.NewGuid());
            await aggregate.SetFunction(new SerializableFunc<int, int>(_ => _));

            var itemsToSend = new List<int> {1, 5, 32, -12};

            var provider = GrainClient.GetStreamProvider(StreamProvider);
            var inputAggregate = new StreamMessageSenderComposite<int>(provider, 2);

            await aggregate.SetInput(await inputAggregate.GetOutputStreams());

            Assert.AreEqual(2, (await aggregate.GetOutputStreams()).Count);

            var consumerAggregate = new TestTransactionalTransactionalStreamConsumerAggregate<int>(provider);
            await consumerAggregate.SetInput(await aggregate.GetOutputStreams());

            var tid = TransactionGenerator.GenerateTransactionId();
            await inputAggregate.StartTransaction(tid);
            await inputAggregate.SendMessage(new ItemMessage<int>(itemsToSend));
            await inputAggregate.EndTransaction(tid);

            var resultItems = consumerAggregate.Items;

            Assert.AreEqual(4, resultItems.Count);
            CollectionAssert.AreEquivalent(itemsToSend, resultItems);

            Assert.IsFalse(await consumerAggregate.AllConsumersTearDownCalled());
            await inputAggregate.TearDown();
            Assert.IsTrue(await consumerAggregate.AllConsumersTearDownCalled());
        }

        /// <summary>
        ///     Validates that no data flows along the chain of streams. Subscription removed is only checked for client
        ///     implementation.
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task TestAggregateCleanupSuccessful()
        {
            var aggregate = GrainClient.GrainFactory.GetGrain<IStreamProcessorSelectAggregate<int, int>>(Guid.NewGuid());
            await aggregate.SetFunction(new SerializableFunc<int, int>(_ => _));

            var itemsToSend = new List<int> {1, 5, 32, -12};

            var provider = GrainClient.GetStreamProvider(StreamProvider);
            var inputAggregate = new StreamMessageSenderComposite<int>(provider, 2); ;

            await aggregate.SetInput(await inputAggregate.GetOutputStreams());

            var streamIdentitiesProcessor = await aggregate.GetOutputStreams();
            Assert.AreEqual(2, (await aggregate.GetOutputStreams()).Count);

            var consumerAggregate = new TestTransactionalTransactionalStreamConsumerAggregate<int>(provider);
            await consumerAggregate.SetInput(await aggregate.GetOutputStreams());

            var subscriptionHdl1 = await GetStreamSubscriptionHandles<IStreamMessage>(streamIdentitiesProcessor[0]);
            var subscriptionHdl2 = await GetStreamSubscriptionHandles<IStreamMessage>(streamIdentitiesProcessor[1]);

            Assert.AreEqual(1, subscriptionHdl1.Count);
            Assert.AreEqual(1, subscriptionHdl2.Count);

            await inputAggregate.TearDown();

            var tId = Guid.NewGuid();
            await inputAggregate.StartTransaction(tId);
            await inputAggregate.EndTransaction(tId);

            var taskCompleted = consumerAggregate.TransactionComplete(tId).Wait(TimeSpan.FromSeconds(5));

            Assert.IsFalse(taskCompleted);

            subscriptionHdl1 = await GetStreamSubscriptionHandles<IStreamMessage>(streamIdentitiesProcessor[0]);
            subscriptionHdl2 = await GetStreamSubscriptionHandles<IStreamMessage>(streamIdentitiesProcessor[1]);

            Assert.AreEqual(0, subscriptionHdl1.Count);
            Assert.AreEqual(0, subscriptionHdl2.Count);
        }


        private async Task<IList<StreamSubscriptionHandle<T>>> GetStreamSubscriptionHandles<T>(StreamIdentity streamIdentity)
        {
            var result = await GrainClient.GetStreamProvider(StreamProvider)
                .GetStream<T>(streamIdentity.Guid, streamIdentity.Namespace)
                .GetAllSubscriptionHandles();

            return result;
        }
    }
}