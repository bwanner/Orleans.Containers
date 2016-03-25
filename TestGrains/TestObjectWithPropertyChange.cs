using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Orleans.Collections.Observable;

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

        public ObjectIdentifier Identifier { get; }
        public event ContainerElementPropertyChangedEventHandler ContainerPropertyChanged;
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}