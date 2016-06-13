using System;

namespace TestGrains
{
    /// <summary>
    /// Example object for demo purposes.
    /// </summary>
    [Serializable]
    public class DummyIntWithPayload
    {
        private byte[] _dataBytes;

        public DummyIntWithPayload(int value, byte[] dataBytes)
        {
            Value = value;
            _dataBytes = dataBytes;
        }

        public override bool Equals(object other)
        {
            var i = other as DummyIntWithPayload;
            return i?.Value == this.Value;
        }

        public override string ToString()
        {
            return Value.ToString();
        }

        public virtual int Value { get; set; }
    }
}
