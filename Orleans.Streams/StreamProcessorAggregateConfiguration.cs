using System.Collections.Generic;

namespace Orleans.Streams
{
    public class StreamProcessorAggregateConfiguration
    {
        public IList<StreamIdentity> InputStreams { get; }
        public int ScatterFactor { get; }

        public StreamProcessorAggregateConfiguration(IList<StreamIdentity> inputStreams, int scatterFactor = 1)
        {
            InputStreams = inputStreams;
            ScatterFactor = scatterFactor;
        }
    }
}