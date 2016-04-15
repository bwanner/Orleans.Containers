using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Orleans.Streams.Linq.Aggregates;

namespace Orleans.Streams
{
    /// <summary>
    /// Creates stream processing aggregates.
    /// </summary>
    public interface IStreamProcessorAggregateFactory
    {
        IGrainFactory GrainFactory { get; }
    }
}