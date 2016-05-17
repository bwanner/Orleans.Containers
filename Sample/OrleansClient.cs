using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Orleans;
using Orleans.Collections;
using Orleans.Collections.Utilities;
using Orleans.Streams;
using Orleans.Streams.Endpoints;
using Orleans.Streams.Linq;
using Orleans.Streams.Messages;
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

            var consumer = new TransactionalStreamListConsumer<ContainerElement<DummyInt>>(provider);
            await consumer.SetInput(await container.GetOutputStreams());

            var transactionId = await container.EnumerateToSubscribers();
            await consumer.TransactionComplete(transactionId);

            Console.WriteLine(consumer.Items);


            // Sample with observable collection and data query.
            var collection = GrainClient.GrainFactory.GetGrain<IContainerGrain<DummyInt>>(Guid.NewGuid());
            int numContainers = 4;
            await collection.SetNumberOfNodes(numContainers);

            var aggregateFactory = new DefaultStreamProcessorAggregateFactory(GrainClient.GrainFactory);
            var query = await collection.Select(x => x.Item.Value, aggregateFactory).Where(x => x > 500);

            var matchingItemConsumer = new TransactionalStreamListConsumer<int>(provider);
            await matchingItemConsumer.SetInput(await query.GetOutputStreams());

            var observedCollectionConsumer = new TransactionalStreamListConsumer<ContainerElement<DummyInt>>(provider);
            await observedCollectionConsumer.SetInput(await collection.GetOutputStreams());

            var inputList = Enumerable.Range(0, 1000).Select(x => new DummyInt(x)).ToList();
            await collection.BatchAdd(inputList);

            Console.WriteLine("#Items: {0}", observedCollectionConsumer.Items.Count);
            Console.WriteLine("#Items resultQuery: {0}", matchingItemConsumer.Items.Count);

            // Simple query using stream provider and consumer.

            var simpleProvider = new StreamMessageSenderComposite<int>(provider, numberOfOutputStreams: 10);

            var queryNumbersLessThan1000 = await simpleProvider.Where(x => x < 1000, aggregateFactory);
            
            var simpleResultConsumer = new TransactionalStreamListConsumer<int>(provider);
            await simpleResultConsumer.SetInput(await queryNumbersLessThan1000.GetOutputStreams());

            var rand = new Random(123);
            await simpleProvider.SendMessage(new ItemMessage<int>(Enumerable.Repeat(2000, 10000).Select(x => rand.Next(x)).ToList()));
            
            Console.WriteLine("#Items less than 1000: {0}", simpleResultConsumer.Items.Count);
        }
    }
}