﻿using System;
using System.Threading.Tasks;

namespace Orleans.Streams.Messages
{
    /// <summary>
    /// Holds information about a transaction within a stream.
    /// </summary>
    [Serializable]
    public struct TransactionMessage : IEquatable<TransactionMessage>, IStreamMessage
    {
        public TransactionState State;
        public Guid TransactionId;

        public bool Equals(TransactionMessage other)
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