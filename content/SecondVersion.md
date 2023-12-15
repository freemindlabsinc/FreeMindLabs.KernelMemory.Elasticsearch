# Introduction

This article is part of a series of articles that will guide readers to create their own connectors for Kernel Memory. 
Similar code can be used to target any other vector database or storage system in the market.

Microsoft currently provides connectors for the following storage systems:

- [Azure AI Search](https://azure.microsoft.com/products/ai-services/ai-search)

<div align="center">
  <img src="images/Connectors.jpg" width="60%" />
</div>

- [Qdrant](https://qdrant.tech)
- Volatile, in-memory KNN records.

In this article we will begin to create a connector for [Elasticsearch](https://www.elastic.co/elasticsearch/).

We will look into [IMemoryDb](https://github.com/microsoft/kernel-memory/blob/main/service/Abstractions/MemoryStorage/IMemoryDb.cs) and the related [MemoryRecord](https://github.com/microsoft/kernel-memory/blob/main/service/Abstractions/MemoryStorage/MemoryRecord.cs). 

At the end of this article we will have an almost-complete connector that can be used to create indices and then store and retrieve vectors and payloads from Elasticsearch.

>*In the next article we will complete the connector by adding support for  [MemoryFilter](https://github.com/microsoft/kernel-memory/blob/main/service/Abstractions/Models/MemoryFilter.cs), which will allow the connector to (pre)filter datasets in multiple ways (i.e. key-based, full-text and semantic), and will enable powerful search that extend semantic search.*

The repository associated to this article is located [here](https://www.github.com/freemindlabsinc/FreeMindLabs.SemanticKernel).


## Why Elasticssearch

Elasticsearch supports KNN querying natively in both cloud and on-premise installations.

[TBC]


# What is IMemoryDb?

The IMemoryDb interface plays a pivotal role in Kernel Memory, serving as the gateway to store and retrieve vectors and payloads from the indices of a vector database or other storage systems capable of performing vector similarity searches.

<div align="center">
  <img src="images/Placeholder_upcoming_content.jpeg" width="20%" />
</div>

## The IMemoryDb interface

The [IMemoryDb](https://github.com/microsoft/kernel-memory/blob/main/service/Abstractions/MemoryStorage/IMemoryDb.cs) interface has seven methods, which can be thought as divided in three groups:

1. Index management
2. Data manipulation
3. Search

## Index management
These three methods allow to create, list and delete indices:

```csharp
Task CreateIndexAsync(string index, int vectorSize, CancellationToken ancellationToken)

Task<IEnumerable<string>> GetIndexesAsync(CancellationToken cancellationToken)

Task DeleteIndexAsync(string index, CancellationToken ancellationToken)
```

Here are some consideration on the arguments of those methods:

- **index**: is the name of the index/collection to create. The name is unique and can be case sensitive in some vector databases. Pay attention when picking names for your indices.

- **vectorSize**: the size of the vectors to be stored in the index/collection. The size of the vectors is determined by the vectorization algorithm used to convert text into vectors. 
  - [OpenAI](https://www.openai.com) uses vectors with 1536 dimentiones.
  - [SBERT.net](https://sbert.net)'s all-MiniLM-L6-v2 uses vectors with 384 dimensions.

In addition, notice that (at the time of writing) the result of GetIndexesAsync is an ```IEnumerable<string>```, which means that the method returns the full list of indices at once. Be careful when calling this method on vector databases that contain a large number of indices.

## Data manipulation

The next two methods allow to update and delete the information inside indices using the data structure [MemoryRecord](https://github.com/microsoft/kernel-memory/blob/main/service/Abstractions/MemoryStorage/MemoryRecord.cs).

### MemoryRecord

MemoryRecord is a class that contains vector information and its related payload. It is defined as follows:

```csharp
public class MemoryRecord
{
    /// <summary>
    /// Unique record ID
    /// </summary>
    [JsonPropertyName("id")]
    [JsonPropertyOrder(1)]
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Embedding vector
    /// </summary>
    [JsonPropertyName("vector")]
    [JsonPropertyOrder(100)]
    [JsonConverter(typeof(Embedding.JsonConverter))]
    public Embedding Vector { get; set; } = new();

    /// <summary>
    /// Optional Searchable Key=Value tags (string => string[] collection)
    ///
    /// Multiple values per keys are supported.
    /// e.g. [ "Collection=Work", "Project=1", "Project=2", "Project=3", "Type=Chat", "LLM=AzureAda2" ]
    ///
    /// Use cases:
    ///  * collections, e.g. [ "Collection=Project1", "Collection=Work" ]
    ///  * folders, e.g. [ "Folder=Inbox", "Folder=Spam" ]
    ///  * content types, e.g. [ "Type=Chat" ]
    ///  * versioning, e.g. [ "LLM=AzureAda2", "Schema=1.0" ]
    ///  * etc.
    /// </summary>
    [JsonPropertyName("tags")]
    [JsonPropertyOrder(2)]
    public TagCollection Tags { get; set; } = new();

    /// <summary>
    /// Optional Non-Searchable payload processed client side.
    ///
    /// Use cases:
    ///  * citations
    ///  * original text
    ///  * descriptions
    ///  * embedding generator name
    ///  * URLs
    ///  * content type
    ///  * timestamps
    ///  * etc.
    /// </summary>
    [JsonPropertyName("payload")]
    [JsonPropertyOrder(3)]
    public Dictionary<string, object> Payload { get; set; } = new();
}
```

To understant MemoryRecord, we need to think Retrival Augmented Generation (RAG).

Say we have a large text document (e.g. 2Mb), and we want to store it in a vector database for later retrieval or AI generation/completion.

- First we need to extract the text from the PDF. 
  - Kernel Memory does this for us through the use of classes such as [PdfDecoder](https://github.com/microsoft/kernel-memory/blob/main/service/Core/DataFormats/Pdf/PdfDecoder.cs).
  - PdfDecoder is one of the many decorders used by the [TextExtractionHandler](https://github.com/microsoft/kernel-memory/blob/main/service/Core/Handlers/TextExtractionHandler.cs).

- When we upload textual data to a vector database, we have to first split the complete text into smaller 'chunks'. *Each chunk becomes a MemoryRecord.*
  - Often chunks are sentences, but they can be paragraphs, or even smaller pieces of text. 
  - The idea is that we don 't want to upload the whole text as a single vector, because that would make it impossible to search for specific parts of the text.

  - KernelMemory does this for us, behind the scenes. 
    - When we upload text, KernelMemory will split it into smaller pieces, and then convert each piece into a vector. It will then also add information such as the source document, the position of the chunk in the document, etc. inside the property ```payload``` which is an array of key-value pairs.

### A MemoryRecord in Elasticsearch

This is how a memory record structure is translated in an Elasticsearch index mapping.

<div align="center">
  <img src="images/Mappings.jpg" width="50%" />
</div>


And this is the data stored in one of such memory record:

<div align="center">
  <img src="images/DataPage1.jpg" width="50%" />
  <img src="images/DataPage2.jpg" width="50%" />
</div>

> TODO: Ask Davis for some screenshots/data from the Azure services that shows similar stuff


What we store and search in vector databases are collections of MemoryRecords.
These collection tend to become populated with a large number of records, and they are often sharded across multiple nodes.

### UpsertAsync and DeleteAsync

The methods to insert-update and delete MemoryRecords are defined as follows:

```csharp
Task<string> UpsertAsync(string index, MemoryRecord record, CancellationToken ancellationToken)

Task DeleteAsync(string index, MemoryRecord record, CancellationToken ancellationToken)
```

Here are some considerations:

- There is no distinction between inserting and updating a record.
  - If the record exists, it will be updated. 
    - The method should return the same value of ```record.Id``` of the submitted record.
  - If it doesn't exist, it will be inserted and a new id will be generated and returned.
    -  [TO VERIFY] what happens to record.id? Is it also updated?

Pay special consideration to the ```Id``` property of MemoryRecord. 
Having ids that are balanced across shards is important for performance reasons.

### Considerations on ids and performance 

> In distributed systems like Elasticsearch, the effectiveness of data retrieval and storage operations is greatly influenced by how the data is distributed across different shards. This distribution is heavily reliant on the mechanism used for generating IDs.

When IDs are poorly generated, such as in sequential order or with common prefixes, it can lead to uneven distribution of data. This uneven distribution results in certain shards bearing a disproportionate amount of the data load, creating hotspots. These hotspots can significantly degrade the performance of the system, as these overloaded shards become bottlenecks for both read and write operations. For example, if sequential IDs like 0001, 0002, 0003... are used, they might all be directed to the same shard, leading to an imbalance in the load distribution.

Conversely, a well-thought-out ID generation strategy can mitigate these issues. By creating IDs that are evenly and randomly distributed, the data is spread more uniformly across all available shards. This can be achieved through algorithms that produce a randomized distribution of IDs, such as hashing algorithms. Such an approach leads to a more balanced shard load, which not only improves the system's efficiency and response times but also enhances its scalability and resilience to high volumes of data and requests.


## Search

The final two methods are used to search the indices and get back the MemoryRecords we stored.

Indices can be searched by using ```filters``` and/or ```text```.

```csharp
IAsyncEnumerable<(MemoryRecord, double)> GetSimilarListAsync(
  string index, 
  string text, 
  ICollection<MemoryFilter>? filters = null, 
  double minRelevance = 0, 
  int limit = 1, 
  bool withEmbeddings = false,
  CancellationToken cancellationToken = default)

IAsyncEnumerable<MemoryRecord> GetListAsync(
  string index,
  ICollection<MemoryFilter>? filters = null,
  int limit = 1,
  bool withEmbeddings = false,
  CancellationToken cancellationToken = default)
```

Here are some considerations:

- Both methods return an ```IAsyncEnumerable``` of MemoryRecords.
  - The Results of a search are not returned all at once, but rather in batches, ordered by a relevance score.
  - The caller can then asynchronously iterate over the results as they are returned.
  
  


## Considerations and possible improvements to IMemoryDb

1. Upsert and DeleteAsync should be able to accept a list of MemoryRecords to be inserted/updated/deleted. This would allow to perform batch operations and improve performance.
2. It would be beneficial to have methods like ES's DeleteByQuery
3. There should be a Delete that just takes the index and the id(s) of the records to be deleted. This would be useful to delete records by id without having to create a MemoryRecord object.
4. GetIndexesAsync should return an ```IAsyncEnumerable<string>``` instead of an ```IEnumerable<string>```. This would allow to return the list of indices in batches, which would be useful when there are a large number of indices.
5. It would be nice if we could have some filters (wildcard at least) in GetIndexesAsync, so that we can filter the list of indices by name.


## The complete implementation of the Elasticsearch connector

> show [code here](https://github.com/freemindlabsinc/FreeMindLabs.SemanticKernel/blob/main/src/ElasticsearchMemoryStorage/ElasticsearchMemory.cs)


## Where do we go from here?

>explain that we will add support for filters in the next article bla bla