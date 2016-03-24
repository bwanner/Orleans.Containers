using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Orleans;
using Orleans.Collections;
using Orleans.Collections.Observable;
using Orleans.Collections.Utilities;
using Orleans.Streams;
using Orleans.Streams.Endpoints;
using Orleans.Streams.Linq;
using TestGrains;
using static Orleans.GrainClient;

namespace CollectionHost
{
    public class OrleansClient
    {
        public async Task Execute()
        {
            var provider = GetStreamProvider("CollectionStreamProvider");

            // Simple collection sample
            var container = GrainClient.GrainFactory.GetGrain<IContainerGrain<DummyInt>>(Guid.NewGuid());
            await container.SetNumberOfNodes(4);
            await container.BatchAdd(Enumerable.Range(0, 10).Select(x => new DummyInt(x)).ToList());

            await container.ExecuteSync((i) => { i.Value += 2; }); // Pass action to container

            var consumer = new MultiStreamListConsumer<ContainerElement<DummyInt>>(provider);
            await consumer.SetInput(await container.GetStreamIdentities());

            var transactionId = await container.EnumerateToStream();
            await consumer.TransactionComplete(transactionId);

            Console.WriteLine(consumer.Items);


            // Sample with observable collection and data query.
            var collection = GrainClient.GrainFactory.GetGrain<IObservableContainerGrain<DummyInt>>(Guid.NewGuid());
            int numContainers = 4;
            await collection.SetNumberOfNodes(numContainers);

            var query = await collection.Select(x => x.Item.Value, GrainClient.GrainFactory).Where(x => x > 500);

            var matchingItemConsumer = new MultiStreamListConsumer<int>(provider);
            await matchingItemConsumer.SetInput(await query.GetStreamIdentities());

            var observedCollectionConsumer = new MultiStreamListConsumer<ContainerElement<DummyInt>>(provider);
            await observedCollectionConsumer.SetInput(await collection.GetStreamIdentities());

            var inputList = Enumerable.Range(0, 1000).Select(x => new DummyInt(x)).ToList();
            await collection.BatchAdd(inputList);

            Console.WriteLine("#Items: {0}", observedCollectionConsumer.Items.Count);
            Console.WriteLine("#Items resultQuery: {0}", matchingItemConsumer.Items.Count);

            // Simple query using stream provider and consumer.

            var simpleProvider = new MultiStreamProvider<int>(provider, numberOutputStreams: 10);

            var queryNumbersLessThan1000 = await simpleProvider.Where(x => x < 1000, GrainClient.GrainFactory);
            
            var simpleResultConsumer = new MultiStreamListConsumer<int>(provider);
            await simpleResultConsumer.SetInput(await queryNumbersLessThan1000.GetStreamIdentities());

            var rand = new Random(123);
            var transaction1 = await simpleProvider.SendItems(Enumerable.Repeat(2000, 10000).Select(x => rand.Next(x)).ToList());

            await simpleResultConsumer.TransactionComplete(transaction1);
            
            Console.WriteLine("#Items less than 1000: {0}", simpleResultConsumer.Items.Count);
        }
    }
}