using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Orleans.Collections.Observable;
using Orleans.Streams;
using Orleans.Streams.Endpoints;
using Orleans.TestingHost;

namespace Orleans.Collections.Test
{
    [TestClass]
    public class ObservableContainerGrainNodeUnitTest : TestingSiloHost
    {
        private IStreamProvider _provider;

        [ClassCleanup]
        public static void ClassCleanup()
        {
            // Optional. 
            // By default, the next test class which uses TestignSiloHost will
            // cause a fresh Orleans silo environment to be created.
            StopAllSilos();
        }

        [TestInitialize]
        public void TestInitialize()
        {
            _provider = GrainClient.GetStreamProvider("CollectionStreamProvider");
        }

        private IObservableContainerNodeGrain<T> GetRandomObservableContainerGrain<T>()
        {
            var grain = GrainFactory.GetGrain<IObservableContainerNodeGrain<T>>(Guid.NewGuid());

            return grain;
        }


        [TestMethod]
        public async Task TestObserveAddItemsToContainer()
        {
            var l = Enumerable.Range(1, 1).ToList();

            var container = GetRandomObservableContainerGrain<int>();

            var resultConsumer = new MultiStreamListConsumer<ContainerElement<int>>(_provider);
            await resultConsumer.SetInput(new List<StreamIdentity>() { await container.GetStreamIdentity()});

            Assert.AreEqual(0, resultConsumer.Items.Count);

            await container.AddRange(l);

            Assert.AreEqual(l.Count, resultConsumer.Items.Count);

        }
    }
}