using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Orleans.Runtime;

namespace Orleans.Streams.Partitioning
{
    public class PartitionGrain : Grain, IPartitionGrain
    {
        private List<Tuple<IGrain, string>> _grains;

        public override Task OnActivateAsync()
        {
            _grains = new List<Tuple<IGrain, string>>();
            DelayDeactivation(TimeSpan.MaxValue);
            return base.OnActivateAsync();
        }

        public Task<IList<Tuple<IGrain, string>>> GetAllGrains()
        {
            IList<Tuple<IGrain, string>> ret = _grains;
            return Task.FromResult(ret);
        }

        public Task RegisterGrain(IGrain grain, string silo)
        {
            _grains.Add(new Tuple<IGrain, string>(grain, silo));
            return TaskDone.Done;
        }
    }
}