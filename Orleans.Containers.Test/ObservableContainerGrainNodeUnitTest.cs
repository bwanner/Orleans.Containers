using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Orleans.Collections.Observable;
using Orleans.Streams;
using Orleans.Streams.Endpoints;
using Orleans.Streams.Messages;
using Orleans.TestingHost;
using TestGrains;

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

        [TestMethod]
        public async Task TestObserveRemoveItemsFromContainer()
        {
            var l = Enumerable.Range(1, 1).ToList();
            var container = GetRandomObservableContainerGrain<int>();

            var resultConsumer = new MultiStreamListConsumer<ContainerElement<int>>(_provider);
            await resultConsumer.SetInput(new List<StreamIdentity>() { await container.GetStreamIdentity() });

            Assert.AreEqual(0, resultConsumer.Items.Count);

            var elementReferences = await container.AddRange(l);
            CollectionAssert.AreEquivalent(l, resultConsumer.Items.Select(i => i.Item).ToList());
            resultConsumer.Items.Clear();

            Assert.AreEqual(l.Count, elementReferences.Count);

            Assert.IsTrue(await container.Remove(elementReferences.First()));
            Assert.AreEqual(1, resultConsumer.Items.Count);
            var deletedReference = resultConsumer.Items.First().Reference;
            Assert.IsFalse(deletedReference.Exists);
            Assert.AreEqual(deletedReference, elementReferences.First());
            Assert.AreEqual(l.Count - 1, await container.Count());
        }

        [TestMethod]
        public async Task TestObserveClearContainer()
        {
            var l = Enumerable.Range(1, 1).ToList();
            var container = GetRandomObservableContainerGrain<int>();

            var resultConsumer = new MultiStreamListConsumer<ContainerElement<int>>(_provider);
            await resultConsumer.SetInput(new List<StreamIdentity>() { await container.GetStreamIdentity() });

            Assert.AreEqual(0, resultConsumer.Items.Count);

            var elementReferences = await container.AddRange(l);
            CollectionAssert.AreEquivalent(l, resultConsumer.Items.Select(i => i.Item).ToList());
            resultConsumer.Items.Clear();
            
            await container.Clear();
            Assert.AreEqual(l.Count, resultConsumer.Items.Count);
            CollectionAssert.AreEquivalent(elementReferences.ToList(), resultConsumer.Items.Select(i => i.Reference).ToList());
            CollectionAssert.AreEquivalent(l, resultConsumer.Items.Select(i => i.Item).ToList());
            Assert.IsTrue(resultConsumer.Items.TrueForAll(i => !i.Reference.Exists));
        }

        [TestMethod]
        public async Task TestPropertyChangedObjectChangeIsAppliedAndForwarded()
        {
            // Stream setup
            var sender = new StreamMessageSender(_provider);

            var container = GetRandomObservableContainerGrain<TestObjectWithPropertyChange>();
            await container.SetInput(await sender.GetStreamIdentity());

            var receiver = new StreamMessageDispatchReceiver(_provider);
            await receiver.Subscribe(await container.GetStreamIdentity());
            var propertyChangedMessages = new List<ItemPropertyChangedMessage>();
            receiver.Register<ItemPropertyChangedMessage>(message =>
            {
                propertyChangedMessages.Add(message);
                return TaskDone.Done;
            });
            var itemMessages = new List<ItemMessage<ContainerElement<TestObjectWithPropertyChange>>>();
            receiver.Register<ItemMessage<ContainerElement<TestObjectWithPropertyChange>>>(message =>
            {
                itemMessages.Add(message);
                return TaskDone.Done;
            });
            // End stream setup

            var testObject = new TestObjectWithPropertyChange(1);

            var hostedElements = await container.AddRange(new List<TestObjectWithPropertyChange>() {testObject});
            Assert.AreEqual(1, itemMessages.Count);
            var hostedElement = itemMessages.First().Items.First();

            var propertyChangedMessage = new ItemPropertyChangedMessage(
                new ContainerElementPropertyChangedEventArgs("Value", 42, testObject.Identifier));
            await sender.SendMessage(propertyChangedMessage);

            // Test if change is applied to collection.
            int newValueInContainer = (int) await container.ExecuteSync(i => i.Value, hostedElement.Reference);
            Assert.AreEqual(42, newValueInContainer);

            Assert.AreEqual(1, propertyChangedMessages.Count);
            var changeEventArgs = propertyChangedMessages.First().ChangedEventArgs;
            Assert.AreEqual(testObject.Identifier, changeEventArgs.ObjectIdentifier);
            Assert.AreEqual(42, changeEventArgs.Value);
            Assert.AreEqual("Value", changeEventArgs.PropertyName);
        }
    }
}