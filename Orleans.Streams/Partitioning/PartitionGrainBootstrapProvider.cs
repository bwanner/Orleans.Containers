using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Orleans.Providers;
using Orleans.Runtime;

namespace Orleans.Streams.Partitioning
{
    public class PartitionGrainBootstrapProvider : IBootstrapProvider
    {
        /// <summary>
        /// Initialization function called by Orleans Provider Manager when a new provider class instance  is created
        /// </summary>
        /// <param name="name">Name assigned for this provider</param>
        /// <param name="providerRuntime">Callback for accessing system functions in the Provider Runtime</param>
        /// <param name="config">Configuration metadata to be used for this provider instance</param>
        /// <returns>Completion promise Task for the inttialization work for this provider</returns>
        public async Task Init(string name, IProviderRuntime providerRuntime, IProviderConfiguration config)
        {
            var partitionGrain = providerRuntime.GrainFactory.GetGrain<IPartitionGrain>(0);
            var siloContextGrain = providerRuntime.GrainFactory.GetGrain<ISiloContextExecutionGrain>(Guid.NewGuid());
            await siloContextGrain.GetSiloIdentity(); // Trigger creation
            await partitionGrain.RegisterGrain(siloContextGrain, providerRuntime.SiloIdentity);
        }

        /// <summary>Close function for this provider instance.</summary>
        /// <returns>Completion promise for the Close operation on this provider.</returns>
        public Task Close()
        {
            return TaskDone.Done;
        }

        /// <summary>The name of this provider instance, as given to it in the config.</summary>
        public string Name => "PartitionGrain";
    }
}
