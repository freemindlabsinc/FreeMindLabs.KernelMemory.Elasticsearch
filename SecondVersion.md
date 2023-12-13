# Introduction

This article is the first of a short series that will lead readers to create a custom implementation of the IMemoryDb interface so that we can use Elasticsearch as a vector database in Kernel Memory.

Similar code can be used to target any other vector database or storage system in the market.

> reference other Microsoft connectors

We will look into IMemoryDb and the related classes such as MemoryRecord and MemoryFilter.
At the end of this article we will have an almost-complete connector that can be used to create indices and then store and retrieve vectors and payloads from Elasticsearch.

In the next article we will complete the connector by adding support for memory filters.

You'll find the source code for this tutorial in this Free Mind Labs' repository.

> :hammer: make sure ES connector repo is clean and don't forget to change its name from FreeMindLabs.SemanticKernel to FreeMindLabs.Kernelmemory.Connectors.Elasticsearch 
so that can be easily found in nuget]**

# What is IMemoryDb?

The IMemoryDb interface plays a pivotal role in Kernel Memory, serving as the gateway to store and retrieve vectors and payloads from the indices of a vector database or other storage systems capable of performing vector similarity searches.

>TODO: put some diagram/image here
considerations about sql systems vs vector dbs

## The IMemoryDb interface

The [IMemoryDb](https://github.com/microsoft/kernel-memory/blob/main/service/Abstractions/MemoryStorage/IMemoryDb.cs) interface has 7 methods, which can be thought as divided in 3 groups:

The first three methods are for **index management**: they allow to create, list and delete indices:

```csharp
Task CreateIndexAsync(string index, int vectorSize, CancellationToken ancellationToken)

Task<IEnumerable<string>> GetIndexesAsync(CancellationToken cancellationToken)

Task DeleteIndexAsync(string index, CancellationToken ancellationToken)
```

The next two methods are for **data management**. 

They allow to update and delete the information inside indices using the data structure [MemoryRecord](https://github.com/microsoft/kernel-memory/blob/main/service/Abstractions/MemoryStorage/MemoryRecord.cs). 
We will look into MemoryRecord in detail in the next section. 
For now, it's enough to know that at the core, MemoryRecord is a vector plus its payload, where the vector is a float array and the payload is a string.

> redacted MemoryRecord here

What we store and search in vector databases are collections of MemoryRecords.


```csharp
Task<string> UpsertAsync(string index, MemoryRecord record, CancellationToken ancellationToken)

Task DeleteAsync(string index, MemoryRecord record, CancellationToken ancellationToken)
```

>make comment on the fact we cannot send batches of records to be inserted/updated/deleted. This is a limitation of the interface that should be addressed in the future.

The final two methods are instead used to **search** the indices and get back the MemoryRecords we store. Indices can be serched semantically or by using filters.

```csharp

IAsyncEnumerable<(MemoryRecord, double)> GetSimilarListAsync(string index, string text, ICollection<MemoryFilter>? filters = null, double minRelevance = 0, int limit = 1, bool withEmbeddings = false, CancellationToken cancellationToken = default)

IAsyncEnumerable<MemoryRecord> GetListAsync(string index, ICollection<MemoryFilter>? filters = null, int limit = 1, bool withEmbeddings = false, CancellationToken cancellationToken = default)
```

We'll cover each of these methods in detail in the following sections.

## Index management

>show examples of storage-specific indices
    -a few pictures are worth a thousand words
    - Create screenshots from my ES Connector showing the indices (mappings) created by the ES connector
    - Ask Davis for some screenshots/data from the Azure services that shows similar stuff

### CreateIndexAsync

Use this method to create an index/collection.

```csharp
Task CreateIndexAsync(string index, int vectorSize, CancellationToken ancellationToken = default);
```

- indexName: is the name of the index/collection to create.
> add notes about case sensitivity and other limitations
- vectorSize: the size of the vectors to be stored in the index/collection.

> show a couple of examples with OpenAI encodings (1536 dimensions) and one with SBERT.net ones (384 dimensions)
> exceptions might get thrown if the index already exists, if the vector size is not supported by the storage system, etc.

### GetIndexesAsync

Use this method to list the indices/collections in the vector database.

```csharp 
Task<IEnumerable<string>> GetIndexesAsync(CancellationToken cancellationToken = efault);
```

> make comment about lack of parameters to filter the results. At the very least some kind of parameter like 'indexnameMask' would be useful to filter the results. 'test-indices.*' would return all indices starting with 'test-indices.' for example.

### DeleteIndexAsync

Use this method to delete an index/collection.

```csharp
Task DeleteIndexAsync(string index, CancellationToken cancellationToken = efault);
```

> make comment about the lack of a result that indicates if it succeeded or not without an exception or a silent failure. I think by returning a boolean we can make this method less ambiguous?

## Data management

To understand data management, we need to look at the MemoryRecord data structure.

### A look at MemoryRecord

> code goes here
> explain the properties

### UpsertAsync

Use this method to insert or update a vector + payload.

```csharp
Task<string> UpsertAsync(string index, MemoryRecord record, CancellationToken ancellationToken = default);
```

- indexName: is the name of the index/collection to update.
- record: is the vector + payload to be stored.
- cancellationToken: is the cancellation token.
- returns: the record ID.

> notes about exceptions.
notes about the fact that the record ID is returned. This is useful for the caller to know the ID of the record that was inserted/updated.
> considerations on ids from storage systems and sharding. 
Do the ids allow for balanced sharding?
 

## Querying

>explain we don't get exact results from this types of search, but rather a list of best matching responses.
Stocacity/temperature/etc.

### MemoryFilter

> explain what a MemoryFilter is and how it works

### GetSimilarListAsync

This method is used to perform a semantic search on the index/collection.

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
- minRelevance: the minimum relevance score for the results.
- limit: the maximum number of results to return.
- withEmbeddings: whether to return the embeddings of the results.
>complete above

### GetListAsync

This method is used to retrieve a list of records from the index/collection using MemoryFilters.

> Ask Davis how he thinks is best to describe this method. It's virtually identical to the method above, except it doesn't take a text parameter. Couldn't we simply make text optional in the method above?

```csharp
IAsyncEnumerable<MemoryRecord> GetListAsync(
       string index,
       ICollection<MemoryFilter>? filters = null,
       int limit = 1,
       bool withEmbeddings = false,
       CancellationToken cancellationToken = default);
```

> write more

### DeleteAsync

```csharp
Task DeleteAsync(string index, MemoryRecord record, CancellationToken ancellationToken = default);
```

> write more


## The complete implementation of the Elasticsearch connector

> show [code here](https://github.com/freemindlabsinc/FreeMindLabs.SemanticKernel/blob/main/src/ElasticsearchMemoryStorage/ElasticsearchMemory.cs)


## Where do we go from here?

>explain that we will add support for filters in the next article bla bla