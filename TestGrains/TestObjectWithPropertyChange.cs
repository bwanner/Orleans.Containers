using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using Orleans.CodeGeneration;
using Orleans.Collections.ObjectState;
using Orleans.Collections.Observable;
using Orleans.Serialization;

namespace TestGrains
{
    [Serializable]
    public class TestObjectWithPropertyChange : DummyInt, IContainerElementNotifyPropertyChanged
    {
        private int _value;

        public override int Value
        {
            get { return _value; }
            set
            {
                _value = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Value"));
                //ContainerPropertyChanged?.Invoke(new ContainerElementPropertyChangedEventArgs("Value", _value, Identifier));
            }
        }

        public TestObjectWithPropertyChange(int value) : base(value)
        {
        }

        [field: NonSerialized]
        public event PropertyChangedEventHandler PropertyChanged;
    }

    public class TestObjectWithPropertyChangeRecursive : IContainerElementNotifyPropertyChanged
    {
        private TestObjectWithPropertyChange _innerItem;

        public TestObjectWithPropertyChange InnerItem
        {
            get { return _innerItem; }
            set
            {
                _innerItem = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("InnerItem"));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }

    [Serializable]
    public class TestObjectListWithPropertyChange : IContainerElementNotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        [property: ContainerNotifyCollectionChanged]
        public ObservableCollection<TestObjectWithPropertyChange> NotifyCollectionSupportingList { get; private set; }
        public ObservableCollection<TestObjectWithPropertyChange> SimpleList { get; private set; }

        public TestObjectListWithPropertyChange()
        {
            NotifyCollectionSupportingList = new ObservableCollection<TestObjectWithPropertyChange>();
            SimpleList = new ObservableCollection<TestObjectWithPropertyChange>();
        }
    }
}