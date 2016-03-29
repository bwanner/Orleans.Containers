using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Orleans.Collections.Observable;
using Orleans.Collections.Utilities;
using Orleans.Streams;
using Orleans.Streams.Endpoints;
using Orleans.Streams.Linq;
using Orleans.TestingHost;
using TestGrains;

namespace Orleans.Collections.Test
{
    [TestClass]
    public class ContainerGrainUnitTest : TestingSiloHost
    {
        private IStreamProvider _provider;


        [TestInitialize]
        public void TestInitialize()
        {
            _provider = GrainClient.GetStreamProvider("CollectionStreamProvider");
        }

        [ClassCleanup]
        public static void ClassCleanup()
        {
            // Optional. 
            // By default, the next test class which uses TestignSiloHost will
            // cause a fresh Orleans silo environment to be created.
            StopAllSilos();
        }

        private IContainerGrain<T> GetRandomDistributedCollection<T>()
        {
            var grain = GrainFactory.GetGrain<IContainerGrain<T>>(Guid.NewGuid());

            return grain;
        }

        [TestMethod]
        public async Task ExecuteLambdaEmpty()
        {
            var distributedCollection = GetRandomDistributedCollection<DummyInt>();
            await distributedCollection.ExecuteSync(x => { ; });
        }

        [TestMethod]
        [Ignore]
        public async Task ExecuteLambdaIncrement()
        {
            var distributedCollection = GetRandomDistributedCollection<DummyInt>();

            var l = (new List<int>() { int.MinValue, 0, 231, -1, 23 }).Select(x => new DummyInt(x)).ToList();
            await distributedCollection.BatchAdd(l);
            Assert.AreEqual(await distributedCollection.Count(), l.Count);

            await distributedCollection.ExecuteSync(x => { x.Value += 232; });

            var listConsumer = new MultiStreamListConsumer<ContainerElement<DummyInt>>(_provider);
            await listConsumer.SetInput(await distributedCollection.GetStreamIdentities());

            var tid = await distributedCollection.EnumerateToSubscribers();
            await listConsumer.TransactionComplete(tid);

            Assert.AreEqual(l.Count, listConsumer.Items.Count);
            var expectedList = l.Select(x => x.Value + 232).ToList();
            var actualList = listConsumer.Items.Select(x => x.Item.Value).ToList();

            CollectionAssert.AreEquivalent(expectedList, actualList);
        }

        [TestMethod]
        public async Task TestAdd()
        {
            var distributedCollection = GetRandomDistributedCollection<int>();
            await distributedCollection.SetNumberOfNodes(4);

            var l = Enumerable.Range(0, 20000).ToList();

            var references = await distributedCollection.BatchAdd(l);

            CollectionAssert.AllItemsAreNotNull(references); 
            // TODO reference sanity check: Should range form 0 to 20000

            var consumer = new MultiStreamListConsumer<ContainerElement<int>>(_provider);
            await consumer.SetInput(await distributedCollection.GetStreamIdentities());

            var tid = await distributedCollection.EnumerateToSubscribers();
            await consumer.TransactionComplete(tid);

            CollectionAssert.AreEquivalent(l, consumer.Items.Select(x => x.Item).ToList());
        }

        [TestMethod]
        public async Task TestRemove()
        {
            var distributedCollection = GetRandomDistributedCollection<int>();
            await distributedCollection.SetNumberOfNodes(4);

            var l = Enumerable.Range(0, 20000).ToList();

            var references = await distributedCollection.BatchAdd(l);

            var consumer = new MultiStreamConsumer<ContainerElement<int>>(_provider);
            await consumer.SetInput(await distributedCollection.GetStreamIdentities());

            var tid = await distributedCollection.EnumerateToSubscribers();
            await consumer.TransactionComplete(tid);

            var deleteValue = (int) await distributedCollection.ExecuteSync(x => x, references.First());

            Assert.IsTrue(await distributedCollection.Contains(deleteValue));
            await distributedCollection.Remove(references.First());
            Assert.IsFalse(await distributedCollection.Contains(deleteValue));
        }



        //[TestMethod]
        //public async Task CopyToOtherCollection()
        //{
        //    var distributedCollection = GetRandomDistributedCollection<int>();

        //    var l = Enumerable.Range(0, 15000).ToList();

        //    await distributedCollection.BatchAdd(l);

        //    Assert.AreEqual(await distributedCollection.Count(), l.Count);

        //    var otherCollection = GetRandomDistributedCollection<int>();
        //    await distributedCollection.EnumerateItems(await otherCollection.GetItemAdders());

        //    Assert.AreEqual(await otherCollection.Count(), l.Count);

        //    var consumer = new MultiStreamConsumer<ContainerHostedElement<int>>(_provider);
        //    await consumer.SetInput(otherCollection.)
        //    var listConsumer = new ListConsumer<int>(512);
        //    IBatchItemConsumer<int> listConsumerRef = await GrainFactory.CreateObjectReference<IDistributedCollectionReader<int>>(listConsumer);
        //    await otherCollection.EnumerateItemsTo(listConsumerRef);

        //    CollectionAssert.AreEqual(l, listConsumer.List.OrderBy(x => x).ToList());
        //}

        //[TestMethod]
        //public async Task TestContains()
        //{
        //    var distributedCollection = GetRandomDistributedCollection<int>();

        //    var l = Enumerable.Range(5, 2000).ToList();
        //    await distributedCollection.BatchAdd(l);

        //    Assert.IsTrue(await distributedCollection.Contains(37));
        //    Assert.IsFalse(await distributedCollection.Contains(-4));
        //}
    }
}