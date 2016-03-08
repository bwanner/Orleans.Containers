using System;

namespace Orleans.Streams
{
    /// <summary>
    /// Holds information about a transaction within a stream.
    /// </summary>
    [Serializable]
    public struct StreamTransaction : IEquatable<StreamTransaction>
    {
        public TransactionState State;
        public int TransactionId;

        public bool Equals(StreamTransaction other)
        {
            return other.State.Equals(this.State) && other.TransactionId == this.TransactionId;
        }
    }

    /// <summary>
    /// State of the transaction.
    /// </summary>
    public enum TransactionState
    {
        Start, End
    }
}