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
            StopAllSilos();
        }

        [TestMethod]
        public async Task TestTransactionNoItems()
        {
            var processorNodeGuid = Guid.NewGuid();

            var processor = GrainClient.GrainFactory.GetGrain<IStreamProcessorSelectNodeGrain<int, int>>(processorNodeGuid);

            await processor.SetFunction(_ => _);

            var provider = GrainClient.GetStreamProvider(StreamProvider);
            var testProvider = new MultiStreamProvider<int>(provider, 1);
            await processor.SetInput((await testProvider.GetStreamIdentities()).First());

            var testConsumer = new MultiStreamListConsumer<int>(provider);

            await SubscribeConsumer(processor, testConsumer);

            var tid = await testProvider.SendItems(new List<int>());
            await testConsumer.TransactionComplete(tid);

            await testProvider.TearDown();
        }

        [TestMethod]
        public async Task TestItemTransfer()
        {
            var processorNodeGuid = Guid.NewGuid();
            var processor = GrainClient.GrainFactory.GetGrain<IStreamProcessorSelectNodeGrain<int, int>>(processorNodeGuid);
            await processor.SetFunction(_ => _);

            var itemsToSend = new List<int> {-1, 5, 30};


            var provider = GrainClient.GetStreamProvider(StreamProvider);
            var testProvider = new MultiStreamProvider<int>(provider, 1);
            await processor.SetInput((await testProvider.GetStreamIdentities()).First());

            var testConsumer = new MultiStreamListConsumer<int>(provider);
            await SubscribeConsumer(processor, testConsumer);

            var tid = await testProvider.SendItems(itemsToSend);
            await testConsumer.TransactionComplete(tid);

            CollectionAssert.AreEquivalent(itemsToSend, testConsumer.Items);

            await testProvider.TearDown();
        }

        private async Task SubscribeConsumer(IStreamProcessorSelectNodeGrain<int, int> processor, MultiStreamListConsumer<int> testConsumer)
        {
            await testConsumer.SetInput(new List<StreamIdentity>() {await processor.GetStreamIdentity()});
        }

        [TestMethod]
        public async Task TestItemAggregation()
        {
            var aggregate = GrainClient.GrainFactory.GetGrain<IStreamProcessorSelectAggregate<int, int>>(Guid.NewGuid());
            await aggregate.SetFunction(_ => _);

            var itemsToSend = new List<int> {1, 5, 32, -12};

            var provider = GrainClient.GetStreamProvider(StreamProvider);
            var inputAggregate = new MultiStreamProvider<int>(provider, 2);

            await aggregate.SetInput(await inputAggregate.GetStreamIdentities());

            Assert.AreEqual(2, (await aggregate.GetStreamIdentities()).Count);

            var consumerAggregate = new TestTransactionalStreamConsumerAggregate<int>(provider);
            await consumerAggregate.SetInput(await aggregate.GetStreamIdentities());

            var tid = await inputAggregate.SendItems(itemsToSend);
            await consumerAggregate.TransactionComplete(tid);

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
            await aggregate.SetFunction(_ => _);

            var itemsToSend = new List<int> {1, 5, 32, -12};

            var provider = GrainClient.GetStreamProvider(StreamProvider);
            var inputAggregate = new MultiStreamProvider<int>(provider, 2); ;

            await aggregate.SetInput(await inputAggregate.GetStreamIdentities());

            var streamIdentitiesProcessor = await aggregate.GetStreamIdentities();
            Assert.AreEqual(2, (await aggregate.GetStreamIdentities()).Count);

            var consumerAggregate = new TestTransactionalStreamConsumerAggregate<int>(provider);
            await consumerAggregate.SetInput(await aggregate.GetStreamIdentities());

            var subscriptionHdl1 = await GetStreamSubscriptionHandles<IStreamMessage>(streamIdentitiesProcessor[0].StreamIdentifier);
            var subscriptionHdl2 = await GetStreamSubscriptionHandles<IStreamMessage>(streamIdentitiesProcessor[1].StreamIdentifier);

            Assert.AreEqual(1, subscriptionHdl1.Count);
            Assert.AreEqual(1, subscriptionHdl2.Count);

            await inputAggregate.TearDown();

            var tid = await inputAggregate.SendItems(itemsToSend);

            var taskCompleted = consumerAggregate.TransactionComplete(tid).Wait(TimeSpan.FromSeconds(5));

            Assert.IsFalse(taskCompleted);

            subscriptionHdl1 = await GetStreamSubscriptionHandles<IStreamMessage>(streamIdentitiesProcessor[0].StreamIdentifier);
            subscriptionHdl2 = await GetStreamSubscriptionHandles<IStreamMessage>(streamIdentitiesProcessor[1].StreamIdentifier);

            Assert.AreEqual(0, subscriptionHdl1.Count);
            Assert.AreEqual(0, subscriptionHdl2.Count);
        }


        private async Task<IList<StreamSubscriptionHandle<T>>> GetStreamSubscriptionHandles<T>(Tuple<Guid, string> identifier)
        {
            var result = await GrainClient.GetStreamProvider(StreamProvider)
                .GetStream<T>(identifier.Item1, identifier.Item2)
                .GetAllSubscriptionHandles();

            return result;
        }
    }
}