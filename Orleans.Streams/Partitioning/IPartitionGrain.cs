using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Orleans.Runtime;

namespace Orleans.Streams.Partitioning
{
    public interface IPartitionGrain : IGrainWithIntegerKey
    {
        Task<IList<Tuple<IGrain, string>>> GetAllGrains();

        Task RegisterGrain(IGrain grain, string silo);

    }
}