# Introduction

This article provides a guide on creating a custom implementation of the
IMemoryDb interface, allowing Kernel Memory to seamlessly use Elasticsearch
as a vector database.

You'll find the source code for this tutorial in this Free Mind Labs' repository.

> :hammer: make sure ES connector repo is clean and don't forget to change its name from FreeMindLabs.SemanticKernel to FreeMindLabs.Kernelmemory.Connectors.Elasticsearch 
so that can be easily found in nuget]**

# What is IMemoryDb?

The IMemoryDb interface plays a pivotal role in Kernel Memory, serving as the gateway
to store and retrieve vectors and payloads from the indices of a vector database or 
other storage systems capable of performing vector similarity searches.

[TODO: put some diagram/image here]

## IMemoryDb definition

The [IMemoryDb](https://github.com/microsoft/kernel-memory/blob/main/service/Abstractions/MemoryStorage/IMemoryDb.cs) interface has 7 methods, which can be thought as divided in 3 groups:

The first three methods are for **index management**. These methods allow to to create, list and delete indices:
```csharp
1. CreateIndexAsync(string index, int vectorSize, CancellationToken ancellationToken)
1. GetIndexesAsync(CancellationToken ancellationToken)
1. DeleteIndexAsync(string index, CancellationToken ancellationToken)
```

The next two methods are for **data management**. They allow to update and delete the information inside indices using the data structure [MemoryRecord](https://github.com/microsoft/kernel-memory/blob/main/service/Abstractions/MemoryStorage/MemoryRecord.cs)

1. UpsertAsync(string index, MemoryRecord record, CancellationToken ancellationToken)
1. DeleteAsync(string index, MemoryRecord record, CancellationToken ancellationToken)

The final two methods are instead used to perform **semantic search** on the indices. These methods return MemoryRecords that are semantically related to the searched text.

1. GetSimilarListAsync(string index, string text, ICollection<MemoryFilter>? filters = null, double minRelevance = 0, int limit = 1, bool withEmbeddings = false, CancellationToken cancellationToken = default)
1. GetListAsync(string index, ICollection<MemoryFilter>? filters = null, int limit = 1, bool withEmbeddings = false, CancellationToken cancellationToken = default)

We'll cover each of these methods in detail in the following sections.

## Index management

[what are indices for?]
[examples of storage-specific indices]    
    -[Ask Davis for some screenshots/data from the Azure services]    
    -[Create screenshots from my ES Connector]

### CreateIndexAsync

Use this method to create an index/collection.

```csharp
Task CreateIndexAsync(string index, int vectorSize, CancellationToken ancellationToken = default);
```

- indexName: is the name of the index/collection to create.
- vectorSize: the size of the vectors to be stored in the index/collection.

### GetIndexesAsync

Use this method to list the indices/collections in the vector database.

```csharp 
Task<IEnumerable<string>> GetIndexesAsync(CancellationToken cancellationToken = efault);
```

### DeleteIndexAsync

Use this method to delete an index/collection.

```csharp
Task DeleteIndexAsync(string index, CancellationToken cancellationToken = efault);
```

## Data management

### UpsertAsync

Use this method to insert or update a vector + payload.

```csharp
Task<string> UpsertAsync(string index, MemoryRecord record, CancellationToken ancellationToken = default);
```

- indexName: is the name of the index/collection to update.
- record: is the vector + payload to be stored.
- cancellationToken: is the cancellation token.
- returns: the record ID.
 

## Querying

```csharp
IAsyncEnumerable<(MemoryRecord, double)> GetSimilarListAsync(
        string index,
        string text,
        ICollection<MemoryFilter>? filters = null,
        double minRelevance = 0,
        int limit = 1,
        bool withEmbeddings = false,
        CancellationToken cancellationToken = default);
```

- indexName: is the name of the index/collection to create.
- text: the text being searched.
- filters: the filters to be applied to the search. 
  - Values to match in the field used for tagging records the field must be a list of strings

   
- - /// <param name="minRelevance">Minimum Cosine Similarity required</param>
   /// <param name="limit">Max number of results</param>
   /// <param name="withEmbeddings">Whether to include vector in the result</param>
   /// <param name="cancellationToken">Task cancellation token</param>
   /// <returns>List of similar vectors, starting from the most similar</returns>
   IAsyncEnumerable<(MemoryRecord, double)> GetSimilarListAsync(
       string index,
       string text,
       ICollection<MemoryFilter>? filters = null,
       double minRelevance = 0,
       int limit = 1,
       bool withEmbeddings = false,
       CancellationToken cancellationToken = default);

   /// <summary>Gets a list of records having a field matching a given value.</ummary>
   /// <param name="index">Index/Collection name</param>
   /// <param name="filters">Values to match in the field used for tagging records the field must be a list of strings)</param>
   /// <param name="limit">Max number of records to return</param>
   /// <param name="withEmbeddings">Whether to include vector in the result</param>
   /// <param name="cancellationToken">Task cancellation token</param>
   /// <returns>List of records</returns>
   IAsyncEnumerable<MemoryRecord> GetListAsync(
       string index,
       ICollection<MemoryFilter>? filters = null,
       int limit = 1,
       bool withEmbeddings = false,
       CancellationToken cancellationToken = default);

   /// <summary>Deletes a memory record.</summary>
   /// <param name="index">Index/Collection name</param>
   /// <param name="record">Record to delete. Most memory DBs require only the recordID to be set.</param>
   /// <param name="cancellationToken">Task cancellation token</param>
   Task DeleteAsync(string index, MemoryRecord record, CancellationToken ancellationToken = default);

}
```

This code includes the namespace "YourNamespace" and the interface name "IMemoryDb" as requested.
```

This format keeps the interface concise while retaining XML documentation comments for clarity.
