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
            var l = new IncomingChangeProcessor<DummyInt>();
            await l.ProcessItemAddMessage(new ItemAddMessage<DummyInt>(Enumerable.Range(0, 100).Select(x => new DummyInt(x)).ToList()));

            Assert.AreEqual(0, l.KnownObjectCount);
        }

        [TestMethod]
        public async Task TestItemPropertyChangeApplied()
        {
            var l = new IncomingChangeProcessor<TestObjectWithPropertyChange>();
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
            var l = new IncomingChangeProcessor<TestObjectWithPropertyChange>();
            var o = new TestObjectWithPropertyChange(42);
            var objectIdentifier = new ObjectIdentifier(123);

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
            var l = new IncomingChangeProcessor<TestObjectWithPropertyChange>();
            var o = new TestObjectWithPropertyChange(42);
            await l.ProcessItemAddMessage(new ItemAddMessage<TestObjectWithPropertyChange>(new List<TestObjectWithPropertyChange> {o}));

            Assert.AreEqual(1, l.KnownObjectCount);
            Assert.IsTrue(l.IsKnownObject(o.GetIdentifier()));
        }

        [TestMethod]
        public async Task TestRecursivePropertyChanged()
        {
            var l = new IncomingChangeProcessor<TestObjectWithPropertyChangeRecursive>();

            var outer = new TestObjectWithPropertyChangeRecursive();
            var inner = new TestObjectWithPropertyChange(42);
            outer.InnerItem = inner;

            var outerIdentifier = new ObjectIdentifier(123123);
            var innerIdentifier = new ObjectIdentifier(2130123);

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
        }

        [Ignore]
        [TestMethod]
        public async Task TestTwoLevelAddedToKnownObject()
        {
            var l = new IncomingChangeProcessor<List<TestObjectWithPropertyChange>>();
            var objectList = Enumerable.Range(0, 100).Select(i => new TestObjectWithPropertyChange(42)).ToList();
            await
                l.ProcessItemAddMessage(
                    new ItemAddMessage<List<TestObjectWithPropertyChange>>(new List<List<TestObjectWithPropertyChange>> {objectList}));

            Assert.AreEqual(100, l.KnownObjectCount);
            foreach (var o in objectList)
            {
                Assert.IsTrue(l.IsKnownObject(o.GetIdentifier()));
            }
        }

        [Ignore]
        [TestMethod]
        public async Task TestTwoLevelAddAndRemove()
        {
            var l = new IncomingChangeProcessor<List<TestObjectWithPropertyChange>>();
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

        //[TestMethod]
        //public async Task TestItemListChangeApplied()
        //{
        //    var l = new IncomingChangeProcessor<TestObjectListWithPropertyChange>();
        //    var root = new TestObjectListWithPropertyChange();
        //    var o1 = new TestObjectWithPropertyChange(42);
        //    root.NotifyCollectionSupportingList.Add(o1);
        //    root.SimpleList.Add(o1);
        //    await l.ProcessItemAddMessage(new ItemAddMessage<TestObjectListWithPropertyChange>(new List<TestObjectListWithPropertyChange>() { root }));

        //    Assert.IsTrue(l.IsKnownObject(root.GetIdentifier()));
        //    Assert.IsTrue(l.IsKnownObject(o1.GetIdentifier()));
        //    Assert.IsTrue(l.IsKnownObject(root.NotifyCollectionSupportingList.GetIdentifier()));
        //    Assert.IsFalse(l.IsKnownObject(root.SimpleList.GetIdentifier()));
        //    Assert.AreEqual(3, l.KnownObjectCount);

        //    var collectionChangedMessage =
        //        new ItemCollectionChangedMessage(new ContainerElementCollectionChangedEventArgs(root.NotifyCollectionSupportingList.GetIdentifier(),
        //            new object[] { o1 }, NotifyCollectionChangedAction.Remove));
        //    await l.ProcessItemCollectionChangedMessage(collectionChangedMessage);

        //    Assert.AreEqual(2, l.KnownObjectCount);
        //    Assert.AreEqual(0, root.NotifyCollectionSupportingList.Count);
        //    Assert.IsTrue(root.SimpleList.Contains(o1));
        //}

        #endregion
    }
}