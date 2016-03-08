using System;

namespace TestGrains
{
    /// <summary>
    /// Example object for demo purposes.
    /// </summary>
    [Serializable]
    public class DummyInt
    {
        public DummyInt(int value)
        {
            Value = value;
        }

        public override bool Equals(object other)
        {
            var i = other as DummyInt;
            return i?.Value == this.Value;
        }

        public override string ToString()
        {
            return Value.ToString();
        }

        public int Value { get; set; }
    }
}
