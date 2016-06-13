using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Orleans.Streams.Partitioning
{
    public static class PartitionGrainUtil
    {
        public static async Task<ISiloContextExecutionGrain> GetExecutionGrain(string siloIdentifier, IGrainFactory factory)
        {
            var partitionGrain = factory.GetGrain<IPartitionGrain>(0);
            var allExecutionGrains = await partitionGrain.GetAllGrains();
            return (ISiloContextExecutionGrain) allExecutionGrains.Where(tuple => tuple.Item2 == siloIdentifier).Select(tuple => tuple.Item1).Single();
        }

        public static async Task<IList<Tuple<ISiloContextExecutionGrain, string>>> GetExecutionGrains(IGrainFactory factory)
        {
            var partitionGrain = factory.GetGrain<IPartitionGrain>(0);
            var allExecutionGrains = await partitionGrain.GetAllGrains();
            return allExecutionGrains.Select(tuple => new Tuple<ISiloContextExecutionGrain, string>((ISiloContextExecutionGrain) tuple.Item1, tuple.Item2)).ToList();
        }

        public static async Task<IList<string>> GetAllSiloNames(IGrainFactory factory)
        {
            var partitionGrain = factory.GetGrain<IPartitionGrain>(0);
            var allExecutionGrains = await partitionGrain.GetAllGrains();
            return allExecutionGrains.GroupBy(tuple => tuple.Item2).Select(g => g.Key).ToList();
        }
    }
}