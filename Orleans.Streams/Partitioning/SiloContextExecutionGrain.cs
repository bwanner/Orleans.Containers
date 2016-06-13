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
            DelayDeactivation(TimeSpan.MaxValue);
            return base.OnActivateAsync();
        }

        public Task Execute(Action<IGrainFactory> action)
        {
            action(GrainFactory);
            return TaskDone.Done;
        }

        public Task<object> ExecuteFunc(Func<IGrainFactory, object> func)
        {
            return Task.FromResult(func(GrainFactory));
        }

        public async Task<object> ExecuteFunc(Func<IGrainFactory, Task<object>> func)
        {
            return await func(GrainFactory);
        }

        public async Task<object> ExecuteFunc(Func<IGrainFactory, object, Task<object>> func, object state)
        {
            return await func(GrainFactory, state);
        }

        public Task Execute(Action<IGrainFactory, object> action, object state)
        {
            action(GrainFactory, state);
            return TaskDone.Done;
        }

        public Task<string> GetSiloIdentity()
        {
            return Task.FromResult(RuntimeIdentity);
        }
    }
}