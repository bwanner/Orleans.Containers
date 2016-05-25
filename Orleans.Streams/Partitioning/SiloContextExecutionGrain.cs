using System;
using System.Threading.Tasks;
using Orleans.Concurrency;
using Orleans.Placement;

namespace Orleans.Streams.Partitioning
{
    [PreferLocalPlacement]
    public class SiloContextExecutionGrain : Grain, ISiloContextExecutionGrain
    {
        private string _tmp;

        /// <summary>
        /// This method is called at the end of the process of activating a grain.
        /// It is called before any messages have been dispatched to the grain.
        /// For grains with declared persistent state, this method is called after the State property has been populated.
        /// </summary>
        public override Task OnActivateAsync()
        {
            _tmp = RuntimeIdentity;
            return base.OnActivateAsync();
        }

        public Task Execute(Action action)
        {
            action();
            return TaskDone.Done;
        }

        public Task Execute(Action<object> action, object state)
        {
            action(state);
            return TaskDone.Done;
        }

        public Task<string> GetSiloIdentity()
        {
            return Task.FromResult(RuntimeIdentity);
        }
    }
}