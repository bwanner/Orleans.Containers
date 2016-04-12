using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using Orleans.CodeGeneration;
using Orleans.Collections.Observable;
using Orleans.Serialization;

namespace TestGrains
{
    [Serializable]
    public class TestObjectWithPropertyChange : DummyInt, IContainerElementNotifyPropertyChanged, INotifyPropertyChanged
    {
        private int _value;

        public override int Value
        {
            get { return _value; }
            set
            {
                _value = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Value"));
                ContainerPropertyChanged?.Invoke(new ContainerElementPropertyChangedEventArgs("Value", _value, Identifier));
            }
        }

        public TestObjectWithPropertyChange(int value) : base(value)
        {
            Identifier = new ObjectIdentifier(Guid.Empty, Guid.NewGuid());
        }

        public void ApplyChange(ContainerElementPropertyChangedEventArgs change)
        {
            switch (change.PropertyName)
            {
                case "Value":
                    Value = (int) change.Value;
                    break;
            }
        }

        public ObjectIdentifier Identifier { get; private set; }
        [field: NonSerialized]
        public event ContainerElementPropertyChangedEventHandler ContainerPropertyChanged;
        [field: NonSerialized]
        public event PropertyChangedEventHandler PropertyChanged;

        [CopierMethod]
        private static object Copy(object input)
        {
            TestObjectWithPropertyChange o = (TestObjectWithPropertyChange) input;
            return new TestObjectWithPropertyChange(o._value) {Identifier = o.Identifier};
        }


    }
}