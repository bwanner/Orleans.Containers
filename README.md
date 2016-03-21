# Orleans.Containers (pre-alpha)
Orleans.Containers was created to provide a method for storing and processing data in a distributed way.

# Core Features
* Distributed storage of POCO objects via `IContainerGrain`.
* Distributed actor-parallel processing via the stream processing features.
* ObservableContainer: Send items along with information on the container they are placed in to a stream.
* Execute parallel LINQ-queries on data streams (`Select` and `Where` supported for now).
* Weak transactionality: Add data to a data stream and find out when all data is processed.

The project consists of two main parts:
## Orleans.Containers
Containers can store POCO objects. One container consists of multiple container nodes. Each container node is a single grain activation. Contrary do regular collections the container takes ownership of the stored data: Data stored in a container can be accessed and modified by using the corresponding `ContainerElementReference<T>`.

## Orleans.Streams
This extension to Orleans.Streams allows to easily deploy a streaming topology within Orleans for data processing. Data flows between data processors in parallel. Right now the `Select` and `Where` LINQ statements are supported for distributed containers or other providers of streaming data.

### Containers Detailed overview

Data is hosted within a *IContainerGrain*, which itself consists of many *IContainerNodeGrain*s. Each *IContainerGrain* contains exactly the part of a collection for which it is responsible. That means access to objects is sequential within one Container, but parallel for many containers.

Functions to be executed in batch can be passed to *IContainerGrain*. Due to the implementation of Func<T> in C# the grain side needs to have access to the same assembly as the client side passing that function.

A distributed collection can be easily copied to another distributed collection or be retrieved by a client.

## Examples
Initializing a distributed collection of DummyInt objects across two different container grains is as easy as these two lines of code:
 ```cs
 // Setup distributed collection
 var collection = GrainClient.GrainFactory.GetGrain<IContainerGrain<DummyInt>>(Guid.NewGuid());
 await collection.SetNumberOfContainers(2);
```
Now we want to add all numbers from 1 to 10,000 to this newly created collection:
```cs
 // Add elements from 1 to 10000 to this collection.
 await collection.BatchAdd(Enumerable.Range(1, 10000).Select(x => new DummyInt(x)).ToList());
 Console.WriteLine("Collection contains: {0} items", await collection.Count());
 ```
 Notice that the items are spread across two different grains and written to them in parallel. As a next step we want to modify each element which can be easily done with this line:
```cs
 // Execute parallel for each container: Add 10000 to each object's value.
 await collection.ExecuteLambda(x => x.Value += 10000);
 ```
We then would like to transfer these results back to the Orleans client. So we create a `MultiStreamListConsumer<ContainerHostedElement<DummyInt>>` and make it addressable to Orleans in line 2. We then tell the collection to enumerate all items to its output stream.
```cs
// Create consumer for the resulting data.
var consumer = new MultiStreamListConsumer<ContainerHostedElement<DummyInt>>(provider);
await consumer.SetInput(await container.GetStreamIdentities());

var transactionId = await container.EnumerateToStream();
await consumer.TransactionComplete(transactionId); // await the transaction.
```

### Example with querying stream
This example creates a container consisting of 4 nodes. The `DummyInt` objects hosted in that container are then mapped to integers and are filtered for being `> 500`.
The whole processing happens in 4 lanes in parallel:

*ContainerNodeGrain1 -> StreamProcessorSelect1 -> StreamProcessorWhere1 -> StreamConsumer*
*ContainerNodeGrain2 -> StreamProcessorSelect2 -> StreamProcessorWhere2 -> StreamConsumer*
*ContainerNodeGrain3 -> StreamProcessorSelect3 -> StreamProcessorWhere3 -> StreamConsumer*
*ContainerNodeGrain4 -> StreamProcessorSelect4 -> StreamProcessorWhere4 -> StreamConsumer*

```cs
var collection = GrainClient.GrainFactory.GetGrain<IObservableContainerGrain<DummyInt>>(Guid.NewGuid());
int numContainers = 4; // Use 4 containerNodes
await collection.SetNumberOfNodes(numContainers);

// Create query on collection
var query = await collection.Select(x => x.Item.Value, GrainClient.GrainFactory).Where(x => x > 500);

// Setup consumer for retrieving the query result.
var matchingItemConsumer = new MultiStreamListConsumer<int>(provider);
await matchingItemConsumer.SetInput(await query.GetStreamIdentities());

var observedCollectionConsumer = new MultiStreamListConsumer<ContainerHostedElement<DummyInt>>(provider); // Consumes items of type ContainerHostedElement<T>. This type contains a "reference" for accessing the DummyInt object via the container.
await observedCollectionConsumer.SetInput(await collection.GetStreamIdentities());

var inputList = Enumerable.Range(0, 1000).Select(x => new DummyInt(x)).ToList();
await collection.BatchAdd(inputList); // Add items to container

Console.WriteLine("#Items: {0}", observedCollectionConsumer.Items.Count); // Outputs 1000, all items in container
Console.WriteLine("#Items resultQuery: {0}", matchingItemConsumer.Items.Count);
```

### Continuous Stream processing

Once items are added to an observable container or Stream Provider, they are forwarded to an output stream.

```cs
var simpleProvider = new MultiStreamProvider<int>(provider, numberOutputStreams: 10);

var queryNumbersLessThan1000 = await simpleProvider.Where(x => x < 1000, GrainClient.GrainFactory);

var simpleResultConsumer = new MultiStreamListConsumer<int>(provider);
await simpleResultConsumer.SetInput(await queryNumbersLessThan1000.GetStreamIdentities());

var rand = new Random(123);
// Send items
var transaction1 = await simpleProvider.SendItems(Enumerable.Repeat(2000, 10000).Select(x => rand.Next(x)).ToList());

await simpleResultConsumer.TransactionComplete(transaction1);

Console.WriteLine("#Items less than 1000: {0}", simpleResultConsumer.Items.Count);
```

### Transaction Guarantee

When using the StreamConsumer/StreamProducer/Containers it is ensured that all data belonging to a transaction is processed before the transaction returns complete.

### Reusing existing streams

- Queries can be reused even at intermediate result levels.
- A call to `x.TearDown()` where x is some sort of stream data provider will trigger all consumers to unsubscribe.

## Terminology

- StreamConsumer
- StreamProvider
- StreamProcessorChain
- TransactionalStreamIdentity

Additional examples are available in class OrleansClient within the Sample project.

## Installation

Checkout this project, open in Visual Studio and execute project "Sample".

## Backlog

*	 Support communication between items in one distributed collection. (To be tested, possible issue: reentrancy)
*	 Extend support for LINQ queries.
*	 Container balancing / refresh.
* Nice to have: Add support for generic methods in Orleans
