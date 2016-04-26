using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Orleans.Collections.Utilities;
using Orleans.Streams;
using Orleans.Streams.Endpoints;
using Orleans.TestingHost;
using TestGrains;

namespace Orleans.Collections.Test
{
    [TestClass]
    public class ContainerGrainNodeUnitTest : TestingSiloHost
    {
        private IStreamProvider _provider;

        [ClassCleanup]
        public static void ClassCleanup()
        {
            // Optional. 
            // By default, the next test class which uses TestignSiloHost will
            // cause a fresh Orleans silo environment to be created.
            StopAllSilosIfRunning();
        }

        [TestInitialize]
        public void TestInitialize()
        {
            _provider = GrainClient.GetStreamProvider("CollectionStreamProvider");
        }


        private IContainerNodeGrain<T> GetRandomContainerGrain<T>()
        {
            var grain = GrainFactory.GetGrain<IContainerNodeGrain<T>>(Guid.NewGuid());

            return grain;
        }

        //private Mock<ITransactionalStreamConsumer<ContainerHostedElement<T>> CreateTestConsumer<T>(List<int> resultList)
        //{
        //    var consumerMock = new Mock<IDistributedCollectionReader<int>>();
        //    consumerMock.Setup(x => x.ConsumeNextItems(It.IsAny<IEnumerable<Tuple<IElementReference<int>, int>>>()))
        //        .Returns(Task.FromResult(true))
        //        .Callback<IEnumerable<int>>((numbers) => resultList.AddRange(numbers));

        //    return consumerMock;
        //}

        [TestMethod]
        public async Task TestNewContainerIsEmpty()
        {
            var container = GetRandomContainerGrain<int>();
            Assert.AreEqual(0, await container.Count());
        }

        [TestMethod]
        public async Task TestNewContainerWriterReturnsNothing()
        {
            var container = GetRandomContainerGrain<int>();
            var consumer = new MultiStreamListConsumer<ContainerElement<int>>(_provider);
            await consumer.SetInput(await container.GetOutputStreams());
            var tid = await container.EnumerateToSubscribers();

            await consumer.TransactionComplete(tid);
            Assert.AreEqual(0, consumer.Items.Count);
        }

        [TestMethod]
        public async Task TestAddItemsToContainer()
        {
            var l = Enumerable.Range(1, 10).ToList();

            var container = GetRandomContainerGrain<int>();
            var consumer = new MultiStreamListConsumer<ContainerElement<int>>(_provider);
            await consumer.SetInput(await container.GetOutputStreams());

            var reference = await container.AddRange(l);
            
            Assert.AreEqual(10, reference.Count);
            Assert.AreEqual(0, reference.First().Offset);
            Assert.AreEqual(container.GetPrimaryKey(), reference.First().ContainerId);

            var tid = await container.EnumerateToSubscribers();
            await consumer.TransactionComplete(tid);

            CollectionAssert.AreEquivalent(l, consumer.Items.Select(x => x.Item).ToList());
        }

        [TestMethod]
        public async Task TestExecuteSync()
        {
            var l = Enumerable.Range(1, 10).Select(x => new DummyInt(x)).ToList();

            var container = GetRandomContainerGrain<DummyInt>();
            var consumer = new MultiStreamListConsumer<ContainerElement<DummyInt>>(_provider);
            await consumer.SetInput(await container.GetOutputStreams());

            var references = await container.AddRange(l);

            // Action
            await container.ExecuteSync(x => { x.Value += 2; });
            await container.EnumerateToSubscribers();
            l.ForEach(x => x.Value += 2);
            CollectionAssert.AreEquivalent(l.Select(x => x.Value).ToList(), consumer.Items.Select(x => x.Item.Value).ToList());
            consumer.Items.Clear();

            // Action with state
            await container.ExecuteSync((x,s) => { x.Value += (int) s; }, 2);
            await container.EnumerateToSubscribers();
            l.ForEach(x => x.Value += 2);
            CollectionAssert.AreEquivalent(l.Select(x => x.Value).ToList(), consumer.Items.Select(x => x.Item.Value).ToList());
            consumer.Items.Clear();

            // Func
            var resultSyncFunc = await container.ExecuteSync(x => x.Value += 2);
            l.ForEach(x => x.Value += 2);
            CollectionAssert.AreEquivalent(l.Select(x => x.Value).ToList(), resultSyncFunc.ToList());

            // Func with State
            var resultSyncFuncState = await container.ExecuteSync((x,s) => x.Value += (int) s, 2);
            l.ForEach(x => x.Value += 2);
            CollectionAssert.AreEquivalent(l.Select(x => x.Value).ToList(), resultSyncFuncState.ToList());


            var ref1 = references.First();
            // Func with reference
            var resultSyncFuncWithReference = await container.ExecuteSync(x => x.Value += 2, ref1);
            l.First().Value += 2;
            Assert.AreEqual(resultSyncFuncWithReference, l.First().Value);

            // Func with reference and state
            var resultAsyncFuncStateWithReference = await container.ExecuteSync((x, s) => x.Value += (int)s, 2, ref1);
            l.First().Value += 2;
            Assert.AreEqual(resultAsyncFuncStateWithReference, l.First().Value);

        }

        [TestMethod]
        public async Task TestExecuteLambda()
        {
            var l = Enumerable.Range(1, 10).Select(x => new DummyInt(x)).ToList();

            var container = GetRandomContainerGrain<DummyInt>();
            var consumer = new MultiStreamListConsumer<ContainerElement<DummyInt>>(_provider);
            await consumer.SetInput(await container.GetOutputStreams());

            var reference = await container.AddRange(l);

            Assert.AreEqual(10, reference.Count);
            Assert.AreEqual(0, reference.First().Offset);
            Assert.AreEqual(container.GetPrimaryKey(), reference.First().ContainerId);

            var result = await container.ExecuteSync(x => x.Value += 2); // ExecuteLambda((i, o) => i.Value += 2);

            var tid = await container.EnumerateToSubscribers();
            await consumer.TransactionComplete(tid);

            var expectedList = l.Select(x => x.Value + 2).ToList();
            var actualList = consumer.Items.Select(e => e.Item.Value).ToList();

            CollectionAssert.AreEquivalent(expectedList, actualList);
        }

        [TestMethod]
        public async Task TestExecuteBatchLambda()
        {

            var l = Enumerable.Range(1, 10).Select(x => new DummyInt(x)).ToList();

            var container = GetRandomContainerGrain<DummyInt>();
            var consumer = new MultiStreamListConsumer<ContainerElement<DummyInt>>(_provider);
            await consumer.SetInput(await container.GetOutputStreams());

            var reference = await container.AddRange(l);

            Assert.AreEqual(10, reference.Count);
            Assert.AreEqual(0, reference.First().Offset);
            Assert.AreEqual(container.GetPrimaryKey(), reference.First().ContainerId);

            await container.ExecuteSync(x => x.Value += 2);

            var tid = await container.EnumerateToSubscribers();
            await consumer.TransactionComplete(tid);

            var expectedList = l.Select(x => x.Value + 2).ToList();
            var actualList = consumer.Items.Select(e => e.Item.Value).ToList();

            CollectionAssert.AreEquivalent(expectedList, actualList);
        }

        [TestMethod]
        public async Task TestExecuteOnItemReference()
        {
            var item = new DummyInt(5);

            var container = GetRandomContainerGrain<DummyInt>();
            var consumer = new MultiStreamListConsumer<ContainerElement<DummyInt>>(_provider);
            await consumer.SetInput(await container.GetOutputStreams());

            var reference = (await container.AddRange(new List<DummyInt>() {item})).First();

            Assert.IsNotNull(reference);
            Assert.AreEqual(0, reference.Offset);
            Assert.AreEqual(container.GetPrimaryKey(), reference.ContainerId);

            await container.ExecuteSync(i => { i.Value += 2; }, reference);

            var tid = await container.EnumerateToSubscribers();
            await consumer.TransactionComplete(tid);


            Assert.AreEqual(7, consumer.Items.First().Item.Value);
        }

    //    [TestMethod]
    //    public async Task TestCrossContainerCommunication()
    //    {
    //        var container1 = GetRandomContainerGrain<SimpleCommunicator>();
    //        var container2 = GetRandomContainerGrain<SimpleCommunicator>();

    //        await container1.Clear();
    //        await container2.Clear();

    //        SimpleCommunicator communicator1 = new SimpleCommunicator();
    //        SimpleCommunicator communicator2 = new SimpleCommunicator();

    //        var reference1 = await container1.Add(communicator1);
    //        var reference2 = await container2.Add(communicator2);

    //        //Expression<Func<SimpleCommunicator, object, Task>> exp = (communicator, o) => communicator.SendNotification();
    //        //var expNode = exp.ToExpressionNode();

    //        //await container1.ExecuteLambda(expNode, null);

    //        var action = new ContainerAction<SimpleCommunicator>((communicator, state) => communicator.SetListener((ContainerElementReference<SimpleCommunicator>) state),
    //            reference1, reference2);
    //        await action.Execute();

    //        await reference1.ExecuteLambda(async (c) => await c.SendNotification());
    //        //await reference1.ExecuteLambda((c, o) =>
    //        //{
    //        //    c.SetListener((IElementReference<SimpleCommunicator>)o);
    //        //    return TaskDone.Done;
    //        //}, reference2);

    //        var curCount = await reference2.ExecuteLambda((c => c.ReceiveCount));
    //        Assert.AreEqual(curCount, 0);

    //        await reference1.ExecuteLambda(async (c) => await c.SendNotification());

    //        curCount = await reference2.ExecuteLambda<int>(c => c.ReceiveCount);
    //        Assert.AreEqual(curCount, 1);
    //    }

    //    [TestMethod]
    //    public async Task TestWithinContainerCommunication()
    //    {
    //        var container1 = GetRandomContainerGrain<SimpleCommunicator>();

    //        await container1.Clear();

    //        SimpleCommunicator communicator1 = new SimpleCommunicator();
    //        SimpleCommunicator communicator2 = new SimpleCommunicator();

    //        var reference1 = await container1.Add(communicator1);
    //        var reference2 = await container1.Add(communicator2);

    //        await reference1.ExecuteLambda(async c => await c.SendNotification());
    //        await reference1.ExecuteLambda((c, o) =>
    //        {
    //            c.SetListener((IElementReference<SimpleCommunicator>)o);
    //            return TaskDone.Done;
    //        }, reference2);

    //        var curCount = await reference2.ExecuteLambda(c => c.ReceiveCount);
    //        Assert.AreEqual(curCount, 0);

    //        await reference1.ExecuteLambda(async c => await c.SendNotification());

    //        curCount = await reference2.ExecuteLambda(c => c.ReceiveCount);
    //        Assert.AreEqual(curCount, 1);
    //    }
    }
}
