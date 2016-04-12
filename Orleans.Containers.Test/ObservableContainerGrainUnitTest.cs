using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Orleans.Collections.Observable;
using Orleans.Collections.Utilities;
using Orleans.Streams;
using Orleans.Streams.Endpoints;
using Orleans.TestingHost;
using TestGrains;

namespace Orleans.Collections.Test
{
    [TestClass]
    public class ObservableContainerGrainUnitTest : TestingSiloHost
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
            StopAllSilosIfRunning();
        }

        [TestMethod]
        public async Task TestObservingAdd()
        {
            var collection = GrainClient.GrainFactory.GetGrain<IObservableContainerGrain<DummyInt>>(Guid.NewGuid());
            int numContainers = 2;
            await collection.SetNumberOfNodes(numContainers);

            var observedCollectionConsumer = new MultiStreamListConsumer<ContainerElement<DummyInt>>(_provider);
            await observedCollectionConsumer.SetInput(await collection.GetStreamIdentities());

            var inputList = Enumerable.Range(0, 1000).Select(x => new DummyInt(x)).ToList();
            await collection.BatchAdd(inputList);

            CollectionAssert.AreEquivalent(inputList.Select(x => x.Value).ToList(), observedCollectionConsumer.Items.Select(x => x.Item.Value).ToList());
        }


        [TestMethod]
        public async Task TestObservingDelete()
        {
            var collection = GrainClient.GrainFactory.GetGrain<IObservableContainerGrain<DummyInt>>(Guid.NewGuid());
            int numContainers = 2;
            await collection.SetNumberOfNodes(numContainers);

            var observedCollectionConsumer = new MultiStreamListConsumer<ContainerElement<DummyInt>>(_provider);
            await observedCollectionConsumer.SetInput(await collection.GetStreamIdentities());

            var inputList = Enumerable.Range(0, 1000).Select(x => new DummyInt(x)).ToList();
            var references = await collection.BatchAdd(inputList);
            Assert.AreEqual(inputList.Count, await collection.Count());

            observedCollectionConsumer.Items.Clear();
            var value = (DummyInt) await collection.ExecuteSync(x => x, references.First());
            await collection.Remove(references.First());

            Assert.AreEqual(1, observedCollectionConsumer.Items.Count);
            var receivedItem = observedCollectionConsumer.Items.First();
            Assert.IsFalse(receivedItem.Reference.Exists);
            Assert.AreEqual(value, receivedItem.Item);
            Assert.AreEqual(inputList.Count - 1, await collection.Count());
        }
    }
}