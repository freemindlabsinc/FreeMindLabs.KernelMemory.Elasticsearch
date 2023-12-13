# How to Implement an IMemoryDb Connector with Elasticsearch

## Introduction
In today's data-driven landscape, efficiently managing and querying large datasets is crucial for any
application. This article explores the integration of Kernel Memory with Elasticsearch, offering a 
seamless solution for handling diverse data stores in a project like Videomatic. By combining the 
robust querying capabilities of Elasticsearch with the versatility of Kernel Memory, we can enhance 
data processing and retrieval in complex applications.

## Background
Videomatic, a dynamic project at Free Mind Labs, requires diverse data storage solutions to handle 
various aspects of data management. The primary data storage needs range from Relational Database 
Management Systems (RDBMS) to Document-based Databases (DocumentDb). However, for specific needs 
like analytics, search, and real-time operations, Elasticsearch stands out as an indispensable tool. 
Elasticsearch, known for its powerful full-text search capabilities and real-time analytics, complements 
our primary data storage by providing enhanced functionalities that are crucial for our application's 
performance and scalability.


----

# How to implement an IMemoryDb connector

## Introduction

During the development of Videomatic, we realized that we would need several types of data stores:

1. 1. A primary datastore
    1. RDBMS or DocumentDb    
1. Elasticsearch
    1. Analytics, search, realtime, etc
  


vector database (Qdrant), and a search engine (Elasticsearch).)



needed to be able to store data in a vector database, but we also needed to be able to store data in Elasticsearch.

- Explain the need:
    - We want to be able to use the RAG features of Kernel Memory, but we want to store data in Elastticsearch
    - This allows us to use the same analytic platform as a vector database, thus reducing the learning curve, and giving us access to a whole set of features that are inherent in Elasticseearch


In order to do this, we need to implement an IMemoryDb connector. This is a simple interface that allows us to connect to a vector database, and perform a handful of operations on it.

## The IMemoryDb interface

The IMemoryDb interface is defined as:

```csharp
public interface IMemoryDb
{
    /// <summary>
    /// Create an index/collection
    /// </summary>
    /// <param name="index">Index/Collection name</param>
    /// <param name="vectorSize">Index/Collection vector size</param>
    /// <param name="cancellationToken">Task cancellation token</param>
    Task CreateIndexAsync(
        string index,
        int vectorSize,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// List indexes from the memory DB
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns>List of indexes</returns>
    Task<IEnumerable<string>> GetIndexesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Delete an index/collection
    /// </summary>
    /// <param name="index">Index/Collection name</param>
    /// <param name="cancellationToken">Task cancellation token</param>
    Task DeleteIndexAsync(
        string index,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Insert/Update a vector + payload
    /// </summary>
    /// <param name="index">Index/Collection name</param>
    /// <param name="record">Vector + payload to save</param>
    /// <param name="cancellationToken">Task cancellation token</param>
    /// <returns>Record ID</returns>
    Task<string> UpsertAsync(
        string index,
        MemoryRecord record,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get list of similar vectors (+payload)
    /// </summary>
    /// <param name="index">Index/Collection name</param>
    /// <param name="text">Text being searched</param>
    /// <param name="filters">Values to match in the field used for tagging records (the field must be a list of strings)</param>
    /// <param name="minRelevance">Minimum Cosine Similarity required</param>
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

    /// <summary>
    /// Get list of records having a field matching a given value.
    /// E.g. searching vectors by tag, for deletions.
    /// </summary>
    /// <param name="index">Index/Collection name</param>
    /// <param name="filters">Values to match in the field used for tagging records (the field must be a list of strings)</param>
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

    /// <summary>
    /// Delete a memory record
    /// </summary>
    /// <param name="index">Index/Collection name</param>
    /// <param name="record">Record to delete. Most memory DBs require only the record ID to be set.</param>
    /// <param name="cancellationToken">Task cancellation token</param>
    Task DeleteAsync(
        string index,
        MemoryRecord record,
        CancellationToken cancellationToken = default);
}


```



## Response in Discord

 if you take a look at the repository at https://github.com/freemindlabsinc/FreeMindLabs.SemanticKernel you can get a good idea of how to implement IMemoryDb.

I guess I should write a blog post about this, but here are the high level points:

You have to implement IMemoryDb 
This is the interface that creates/deletes/updates indices (CreateIndex, DeleteIndex, etc) and the data itself (Upsert,Delete,etc).
The same interface also provides methods to query data (methods Search, Ask, etc).

See [ElasticsearchMemory.cs](https://github.com/freemindlabsinc/FreeMindLabs.SemanticKernel/blob/main/src/ElasticsearchMemoryStorage/ElasticsearchMemory.cs)

See also [Azure AI's implementation](https://github.com/freemindlabsinc/kernel-memory/blob/main/service/Core/MemoryStorage/AzureAISearch/AzureAISearchMemory.cs) and the Qdrant one nearby.

You will need a data structure (a MemoryRecord) that essentially acts as your persistence class, used by (1) above.
See (ElasticsearchMemoryRecord.cs](https://github.com/freemindlabsinc/FreeMindLabs.SemanticKernel/blob/main/src/ElasticsearchMemoryStorage/ElasticsearchMemoryRecord.cs)

Also look at the other samples [here](https://github.com/freemindlabsinc/kernel-memory/tree/main/service/Core/MemoryStorage)

Finally you need to take care of filtering. I have not finished this yet (I am working on the Hackathon till Monday afternoon) but if you look at the (Qdrant)[https://github.com/freemindlabsinc/kernel-memory/blob/main/service/Core/MemoryStorage/Qdrant/QdrantMemory.cs] implementation at line 161 (GetListAsync) you shall see how that works.
