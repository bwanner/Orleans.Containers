using System;
using System.Collections.Generic;
using System.Linq;

namespace Orleans.Streams
{
    [Serializable]
    public class StreamProcessorAggregateConfiguration
    {
        public IList<SiloLocationStreamIdentity> InputStreams { get; }
        public int ScatterFactor { get; }

        public StreamProcessorAggregateConfiguration(IList<StreamIdentity> inputStreams, int scatterFactor = 1)
        {
            InputStreams = inputStreams.Select(s => new SiloLocationStreamIdentity(s.Guid, s.Namespace, "")).ToList();
            ScatterFactor = scatterFactor;
        }

        public StreamProcessorAggregateConfiguration(IList<SiloLocationStreamIdentity> inputStreams, int scatterFactor = 1)
        {
            InputStreams = inputStreams;
            ScatterFactor = scatterFactor;
        }
    }
}