using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Orleans.Runtime;
using Orleans.Streams.Endpoints;
using Orleans.Streams.Linq;
using Orleans.Streams.Messages;
using Orleans.Streams.Partitioning;
using Orleans.Streams.Test.Helpers;
using Orleans.TestingHost;

namespace Orleans.Streams.Test
{
    [TestClass]
    public class PartitionTest : TestingSiloHost
    {
        [TestInitialize]
        public void TestInitialize()
        {
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
        public async Task TestPartitionGrainStarted()
        {
            var partitionGrain = GrainFactory.GetGrain<IPartitionGrain>(0);
            var registeredGrains = await partitionGrain.GetAllGrains();
            var activeSilosCount = GetActiveSilos().Count();
            Assert.AreEqual(activeSilosCount, registeredGrains.Count);

            foreach (var registeredGrain in registeredGrains)
            {
                var siloIdentity= await ((ISiloContextExecutionGrain) registeredGrain.Item1).GetSiloIdentity();
                Assert.AreEqual(registeredGrain.Item2, siloIdentity);
            }
        }
    }
}