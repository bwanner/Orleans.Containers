using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Orleans.Collections.Observable;
using TestGrains;

namespace Orleans.Collections.Test
{
    [TestClass]
    public class ObservableContainerElementListTest
    {

        [TestMethod]
        public async Task TestNoInterfaceMatches()
        {
            ObservableContainerElementList<DummyInt> l = new ObservableContainerElementList<DummyInt>(Guid.Empty, null, null);
            await l.AddRange(Enumerable.Range(0, 100).Select(x => new DummyInt(x)).ToList());

            Assert.AreEqual(0, l.KnownObjectCount);
        }

        [TestMethod]
        public async Task TestOneLevelInterfaceMatch()
        {
            var l = new ObservableContainerElementList<TestObjectWithPropertyChange>(Guid.Empty, null, null);
            var o = new TestObjectWithPropertyChange(42);
            await l.AddRange(new List<TestObjectWithPropertyChange> {o});

            Assert.AreEqual(1, l.KnownObjectCount);
            Assert.IsTrue(l.IsKnownObject(o.Identifier));
        }

        [TestMethod]
        public async Task TestTwoLevelInterfaceMatches()
        {
            var l = new ObservableContainerElementList<List<TestObjectWithPropertyChange>>(Guid.Empty, null, null);
            var objectList = Enumerable.Range(0, 100).Select(i => new TestObjectWithPropertyChange(42)).ToList();
            await l.AddRange(new List<List<TestObjectWithPropertyChange>> { objectList});

            Assert.AreEqual(100, l.KnownObjectCount);
            foreach (var o in objectList)
            {
                Assert.IsTrue(l.IsKnownObject(o.Identifier));
            }
        }

        [TestMethod]
        public async Task TestTwoLevelInterfaceMatchesAndRemove()
        {
            var l = new ObservableContainerElementList<List<TestObjectWithPropertyChange>>(Guid.Empty, null, null);
            var objectList1 = Enumerable.Range(0, 100).Select(i => new TestObjectWithPropertyChange(i)).ToList();
            var objectList2 = Enumerable.Range(0, 100).Select(i => new TestObjectWithPropertyChange(i)).ToList();
            var hostedElements = await l.AddRange(new List<List<TestObjectWithPropertyChange>> { objectList1, objectList2 });

            Assert.AreEqual(200, l.KnownObjectCount);
            foreach (var o in objectList1)
            {
                Assert.IsTrue(l.IsKnownObject(o.Identifier));
            }
            foreach (var o in objectList2)
            {
                Assert.IsTrue(l.IsKnownObject(o.Identifier));
            }

            await l.Remove(hostedElements.First());
            Assert.AreEqual(100, l.KnownObjectCount);

            foreach (var o in objectList1)
            {
                Assert.IsFalse(l.IsKnownObject(o.Identifier));
            }
            foreach (var o in objectList2)
            {
                Assert.IsTrue(l.IsKnownObject(o.Identifier));
            }
        }
    }
}