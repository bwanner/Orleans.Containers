using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Orleans.Collections.Messages;
using Orleans.Collections.ObjectState;
using Orleans.Collections.Observable;
using TestGrains;

namespace Orleans.Collections.Test
{
    [TestClass]
    public class IncomingChangeProcessorTest
    {
        #region IContainerNotifyPropertyChanged Tests

        [TestMethod]
        public async Task TestAttributeMissingNotAdded()
        {
            var l = new IncomingChangeProcessor();
            await l.ProcessItemAddMessage(new ItemAddMessage<DummyInt>(Enumerable.Range(0, 100).Select(x => new DummyInt(x)).ToList()));

            Assert.AreEqual(0, l.KnownObjectCount);
        }

        [TestMethod]
        public async Task TestItemPropertyChangeApplied()
        {
            var l = new IncomingChangeProcessor();
            var o = new TestObjectWithPropertyChange(42);
            await l.ProcessItemAddMessage(new ItemAddMessage<TestObjectWithPropertyChange>(new List<TestObjectWithPropertyChange> {o}));

            ObjectIdentityLookup lookup = new ObjectIdentityLookup();
            var propertyChangedMessage =
                new ItemPropertyChangedMessage(new ContainerElementPropertyChangedEventArgs("Value", 12, o.GetIdentifier(), lookup));
            await l.ProcessItemPropertyChangedMessage(propertyChangedMessage);

            Assert.AreEqual(12, o.Value);
            Assert.AreEqual(1, l.KnownObjectCount);
            Assert.IsTrue(l.IsKnownObject(o.GetIdentifier()));
        }

        [TestMethod]
        public async Task TestItemIdentityLookup()
        {
            var l = new IncomingChangeProcessor();
            var o = new TestObjectWithPropertyChange(42);
            var objectIdentifier = new ObjectIdentifier(123, Guid.NewGuid());

            ObjectIdentityLookup lookup = new ObjectIdentityLookup();
            lookup.LookupDictionary.Add(o, objectIdentifier);
            await
                l.ProcessItemAddMessage(new ObservableItemAddMessage<TestObjectWithPropertyChange>(new List<TestObjectWithPropertyChange> {o}, lookup));

            var propertyChangedMessage =
                new ItemPropertyChangedMessage(new ContainerElementPropertyChangedEventArgs("Value", 12, o.GetIdentifier(), lookup));
            await l.ProcessItemPropertyChangedMessage(propertyChangedMessage);

            Assert.AreEqual(12, o.Value);
            Assert.AreEqual(1, l.KnownObjectCount);
            Assert.IsTrue(l.IsKnownObject(objectIdentifier));
        }

        [TestMethod]
        public async Task TestOneLevelAddedToKnownObject()
        {
            var l = new IncomingChangeProcessor();
            var o = new TestObjectWithPropertyChange(42);
            await l.ProcessItemAddMessage(new ItemAddMessage<TestObjectWithPropertyChange>(new List<TestObjectWithPropertyChange> {o}));

            Assert.AreEqual(1, l.KnownObjectCount);
            Assert.IsTrue(l.IsKnownObject(o.GetIdentifier()));
        }

        [TestMethod]
        public async Task TestRecursivePropertyChanged()
        {
            var l = new IncomingChangeProcessor();

            var outer = new TestObjectWithPropertyChangeRecursive();
            var inner = new TestObjectWithPropertyChange(42);
            outer.InnerItem = inner;

            var outerIdentifier = new ObjectIdentifier(1, Guid.NewGuid());
            var innerIdentifier = new ObjectIdentifier(2, Guid.NewGuid());

            ObjectIdentityLookup lookup = new ObjectIdentityLookup();
            lookup.LookupDictionary.Add(outer, outerIdentifier);
            lookup.LookupDictionary.Add(inner, innerIdentifier);

            await
                l.ProcessItemAddMessage(
                    new ObservableItemAddMessage<TestObjectWithPropertyChangeRecursive>(new List<TestObjectWithPropertyChangeRecursive> {outer},
                        lookup));

            Assert.AreEqual(2, l.KnownObjectCount);
            Assert.IsTrue(l.IsKnownObject(outerIdentifier));
            Assert.IsTrue(l.IsKnownObject(innerIdentifier));

            await
                l.ProcessItemPropertyChangedMessage(
                    new ItemPropertyChangedMessage(new ContainerElementPropertyChangedEventArgs("InnerItem", null, outerIdentifier,
                        lookup)));

            Assert.AreEqual(1, l.KnownObjectCount);
            Assert.IsTrue(l.IsKnownObject(outerIdentifier));
            Assert.IsFalse(l.IsKnownObject(innerIdentifier));

            var propertyChangedArgs = new ContainerElementPropertyChangedEventArgs("InnerItem", inner, outerIdentifier, lookup);
            await l.ProcessItemPropertyChangedMessage(new ItemPropertyChangedMessage(propertyChangedArgs));

            Assert.AreEqual(2, l.KnownObjectCount);
            Assert.IsTrue(l.IsKnownObject(outerIdentifier));
            Assert.IsTrue(l.IsKnownObject(innerIdentifier));
        }

        [Ignore]
        [TestMethod]
        public async Task TestTwoLevelAddAndRemove()
        {
            var l = new IncomingChangeProcessor();
            var objectList1 = Enumerable.Range(0, 100).Select(i => new TestObjectWithPropertyChange(i)).ToList();
            var objectList2 = Enumerable.Range(0, 100).Select(i => new TestObjectWithPropertyChange(i)).ToList();
            await
                l.ProcessItemAddMessage(
                    new ItemAddMessage<List<TestObjectWithPropertyChange>>(new List<List<TestObjectWithPropertyChange>> {objectList1, objectList2}));

            Assert.AreEqual(200, l.KnownObjectCount);
            foreach (var o in objectList1)
            {
                Assert.IsTrue(l.IsKnownObject(o.GetIdentifier()));
            }
            foreach (var o in objectList2)
            {
                Assert.IsTrue(l.IsKnownObject(o.GetIdentifier()));
            }

            l.RemoveItems(objectList1);
            Assert.AreEqual(100, l.KnownObjectCount);

            foreach (var o in objectList1)
            {
                Assert.IsFalse(l.IsKnownObject(o.GetIdentifier()));
            }
            foreach (var o in objectList2)
            {
                Assert.IsTrue(l.IsKnownObject(o.GetIdentifier()));
            }
        }

        #endregion

        #region ContainerNotifyCollectionChangedAttribute and IContainerNotifyPropertyChanged Test

        [TestMethod]
        public async Task TestRecursiveCollectionChanged()
        {
            var l = new IncomingChangeProcessor();
            var root = new TestObjectListWithPropertyChange();
            var o1 = new TestObjectWithPropertyChange(42);
            root.NotifyCollectionSupportingList.Add(o1);
            root.SimpleList.Add(o1);

            var rootIdentifier = new ObjectIdentifier(1, Guid.NewGuid());
            var o1Identifier = new ObjectIdentifier(2, Guid.NewGuid());
            var listIdentifier = new ObjectIdentifier(3, Guid.NewGuid());
            ObjectIdentityLookup lookup = new ObjectIdentityLookup();
            lookup.LookupDictionary.Add(root, rootIdentifier);
            lookup.LookupDictionary.Add(o1, o1Identifier);
            lookup.LookupDictionary.Add(root.NotifyCollectionSupportingList, listIdentifier);


            await l.ProcessItemAddMessage(
                new ObservableItemAddMessage<TestObjectListWithPropertyChange>(new List<TestObjectListWithPropertyChange>() { root }, lookup));

            Assert.AreEqual(3, l.KnownObjectCount);
            Assert.IsTrue(l.IsKnownObject(rootIdentifier));
            Assert.IsTrue(l.IsKnownObject(o1Identifier));
            Assert.IsTrue(l.IsKnownObject(listIdentifier));

            // Setup another element in list
            var o2 = new TestObjectWithPropertyChange(12039);
            var o2Identifier = new ObjectIdentifier(4, Guid.NewGuid());
            lookup.LookupDictionary.Add(o2, o2Identifier);

            var collectionChangedMessage =
                new ItemCollectionChangedMessage(new ContainerElementCollectionChangedEventArgs(listIdentifier,
                    new object[] { o2 }, NotifyCollectionChangedAction.Add, lookup));
            await l.ProcessItemCollectionChangedMessage(collectionChangedMessage);

            Assert.AreEqual(4, l.KnownObjectCount);
            Assert.AreEqual(2, root.NotifyCollectionSupportingList.Count);
            Assert.IsTrue(l.IsKnownObject(o2Identifier));

            collectionChangedMessage = new ItemCollectionChangedMessage(new ContainerElementCollectionChangedEventArgs(listIdentifier,
        new object[] { o1, o2 }, NotifyCollectionChangedAction.Remove, lookup));
            await l.ProcessItemCollectionChangedMessage(collectionChangedMessage);

            Assert.AreEqual(2, l.KnownObjectCount);
            Assert.IsTrue(l.IsKnownObject(rootIdentifier));
            Assert.IsTrue(l.IsKnownObject(listIdentifier));
        }

        #endregion
    }
}