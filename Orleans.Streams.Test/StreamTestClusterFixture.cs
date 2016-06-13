using System;
using Orleans.Runtime.Configuration;
using Orleans.Serialization;
using Orleans.TestingHost;

namespace Orleans.Streams.Test
{
    public class StreamTestClusterFixture : IDisposable
    {
        public StreamTestClusterFixture()
        {
            GrainClient.Uninitialize();
            SerializationManager.InitializeForTesting();

            TestClusterOptions opts = new TestClusterOptions(2);
            opts.ClusterConfiguration.LoadFromFile("OrleansConfigurationForTesting.xml");
            opts.ClientConfiguration = ClientConfiguration.LoadFromFile("ClientConfigurationForTesting.xml");
            TestCluster cluster = new TestCluster(opts);

            if (cluster.Primary == null)
            {
                cluster.Deploy();
            }

            cluster.InitializeClient();
        }

        /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}