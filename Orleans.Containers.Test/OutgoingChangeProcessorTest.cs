using Microsoft.VisualStudio.TestTools.UnitTesting;
using Orleans.Collections.Observable;
using TestGrains;

namespace Orleans.Collections.Test
{
    [TestClass]
    public class OutgoingChangeProcessorTest
    {

        #region PropertyChanged

        [TestMethod]
        public void TestOutgoingProcessorPropertyChangeEventFired()
        {
            var processor = new OutgoingChangeProcessor();

            var outer = new TestObjectListWithPropertyChange();
            var inner = new TestObjectWithPropertyChange(42);
            outer.NotifyCollectionSupportingList.Add(inner);

            processor.AddItem(outer);

            Assert.AreEqual(3, processor.KnownObjectCount);

            bool eventInvokedCorrectly = false;
            processor.ContainerPropertyChanged += change =>
            {
                Assert.AreEqual(inner.GetIdentifier(), change.Identifier);
                Assert.AreEqual(0, change.IdentityLookup.LookupDictionary.Count);
                Assert.AreEqual(123, change.Value);
                eventInvokedCorrectly = true;
            };

            inner.Value = 123;

            Assert.IsTrue(eventInvokedCorrectly);
        }

        [TestMethod]
        public void TestOutgoingProcessorMultiLevelPropertyChangeEventFired()
        {
            // TODO implement
            Assert.IsTrue(false);
            var processor = new OutgoingChangeProcessor();

            var outer = new TestObjectListWithPropertyChange();
            var inner = new TestObjectWithPropertyChange(42);
            outer.NotifyCollectionSupportingList.Add(inner);

            processor.AddItem(outer);

            Assert.AreEqual(3, processor.KnownObjectCount);

            bool eventInvokedCorrectly = false;
            processor.ContainerPropertyChanged += change =>
            {
                Assert.AreEqual(inner.GetIdentifier(), change.Identifier);
                Assert.AreEqual(0, change.IdentityLookup.LookupDictionary.Count);
                Assert.AreEqual(123, change.Value);
                eventInvokedCorrectly = true;
            };

            inner.Value = 123;

            Assert.IsTrue(eventInvokedCorrectly);
        }

        #endregion

    }
}