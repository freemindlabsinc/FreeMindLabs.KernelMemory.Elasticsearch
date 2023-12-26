:warning: **This article is a work in progress.** 

# How to build a Kernel Memory connector and use Elasticsearch as vector database - Part 1

**TL;DR:** This article is the first of a series of articles that will guide readers to create their own connectors for [Kernel Memory](https://github.com/microsoft/kernel-memory). It will do so by showcasing how to write a connector for [Elasticsearch](https://www.elastic.co/elasticsearch/). At the end of this article we will have an almost-complete connector that can be used to create indices and then store and retrieve vectors and payloads from Elasticsearch. Similar code can be used to target other storage systems.

## A brief introduction to Kernel Memory

According to [Devis Lucato](https://www.linkedin.com/in/devislucato/) (*Principal Architect at Microsoft - Semantic Kernel & Memory*) we should think about Kernel Memory mostly as a way to:

1. Answer questions.
2. Ground answers exclusively on the data ingested.
3. Provide links and references to the original sources.

To **answer questions** is the main goal of Kernel Memory and that needs to happen **after grounding the answers** on our selected data. Not only this helps avoiding [hallucinations](https://zapier.com/blog/ai-hallucinations/), but Davis stated that "*we fundamentally don't trust AI in autopilot mode. We need a way to audit it.*"

It's implied Kernel memory also provides the functionality to **ingest data** and **index it** in a way that makes it possible to *answer questions*. That is the task of the interface [IMemoryDb](https://github.com/microsoft/kernel-memory/blob/main/service/Abstractions/MemoryStorage/IMemoryDb.cs) which we will discuss later in this article.

---

From a technical point of view, Kernel Memory is an open-source service and plugin specialized in the efficient indexing of datasets through custom continuous data hybrid pipelines. 

<div align="center">
  <img src="images/Pipelines.jpg" width="100%" />
</div> 

Utilizing advanced embeddings and LLMs, the system enables Natural Language querying for obtaining answers from the indexed data, complete with citations and links to the original sources.

<div align="center">
  <img src="images/RAG.jpg" width="100%" />
</div>

It is designed to be used as a service, but it can also be embedded in applications (i.e. serverless model), although with some limitations.

## Connectors
In order to operate, Kernel Memory needs to connect to a vector database or storage system that is capable of performing vector similarity searches.

Microsoft currently provides connectors for the following storage systems:

- [Azure AI Search](https://azure.microsoft.com/products/ai-services/ai-search)
  - See [AzureAISearchMemory.cs on Github](https://github.com/microsoft/kernel-memory/tree/main/service/Core/MemoryStorage/AzureAISearch)

- [Qdrant](https://qdrant.tech)
  - See [QdrantMemory.cs on Github](https://github.com/microsoft/kernel-memory/blob/main/service/Core/MemoryStorage/Qdrant/QdrantMemory.cs)

- [Postgres+pgvector](https://github.com/microsoft/kernel-memory-postgres)
  - See [PostgresMemory.cs on Github](https://github.com/microsoft/kernel-memory-postgres)

- Volatile, in-memory KNN records.
  - See [SimpleVectordb.cs on Github](https://github.com/microsoft/kernel-memory/blob/main/service/Core/MemoryStorage/DevTools/SimpleVectorDb.cs)

## Why Elasticssearch

Elasticsearch supports kNN querying natively in both cloud and on-premise installations. kNN stands for 'K nearest neighbors', and it is a search algorithm that finds the K most similar vectors to a given query vector. Because of this, Elasticsearch is a great candidate for vector database needs in addition to being a great text search engine and real time analytics platform.

>Read more about Elasticsearch kNN search [here](https://www.elastic.co/guide/en/elasticsearch/reference/current/knn-search.html).

Of particular interest is the fact that Elasticsearch's kNN query has a ```filter``` option allow us to pre-filter large chunks of data before performing the kNN search, thus greatly improving overall performance.

If you have millions of records in your index you can use the filter to limit the search to a subset of the data, and then perform the kNN search on that subset. *We will see an example of this shortly.*

kNN search and the ```filter``` option make Elasticsearch a great candidate for vector database needs and also enable hybrid search and real time analytics in one platform. If you use Elasticsearch for text (*lexical*) and key-word search, why not use it also for semantic search?


### A semantic search using Elastic Query Language

Say that we wanted to get the response to the question *"What do you know of carbon?"* from a set of documents we imported in Kernel Memory, such as that used by the tests in the article's repository.

>Since we used our Kernel Memory connector to ES, we know those documents will be stored in an Elasticsearch index. In the examples below it will be the ```default``` index, but it could be named anything you prefer.

The *vectorization of the question* *"What do you know of carbon?"* will produce a value like the following:

```json
// The vector of 'What do you know of carbon?' 
[-0.00891963486,0.0110242674,-0.0150581468, ...]
```

>*Side node: the array size depends on the model used to generate embeddings.
>For instance: Open AI uses 1536 dimensions, while SBERT.net's [all-MiniLM-L6-v2](https://huggingface.co/sentence-transformers/all-MiniLM-L6-v2) uses 384 dimensions.*

We can now use this vector to query an index of documents we previously indexed and get back the 10 most similar vectors to the query using a EQL query like the following:

```json
// Queries the field 'embedding' (array of float) in the index 'default' for the 10 most similar vectors to the question `What do you know of carbon?`.

GET default/_search
{
  "knn": {    
    "field": "embedding",
    "k": 10,
    "num_candidates": 100,
    "query_vector": [-0.00891963486,0.0110242674,-0.0150581468, ...]
  }
}
```

>*To gather results, the kNN search API finds a ```num_candidates number``` of approximate nearest neighbor candidates on each shard. See here for [more details on how kNN search works in Elasticsearch](https://www.elastic.co/guide/en/elasticsearch/reference/current/knn-search.html).*

The results of the query would look like the following (*the value of the field ```embedding``` was truncated*):

```
{
  "took": 0,
  "timed_out": false,
  "_shards": {
    "total": 1,
    "successful": 1,
    "skipped": 0,
    "failed": 0
  },
  "hits": {
    "total": {
      "value": 2,
      "relation": "eq"
    },
    "max_score": 1,
    "hits": [
      {
        "_index": "canimportonedocumentandaskasync",
        "_id": "d=doc001//p=904d05afeeb24c2b99c9ad73fd97e690",
        "_score": 1,
        "_source": {
          "id": "d=doc001//p=904d05afeeb24c2b99c9ad73fd97e690",
          "tags": [
            {
              "name": "__document_id",
              "value": "doc001"
            },
            {
              "name": "__file_type",
              "value": "text/plain"
            },
            {
              "name": "__file_id",
              "value": "0334bca3c83e442ea32f9cac85ff8763"
            },
            {
              "name": "__file_part",
              "value": "d414c1d106524bb2a6eddf51d5b92e18"
            },
            {
              "name": "indexedOn",
              "value": "2023-12-26T21:25:25.885+00:00"
            }
          ],

          "payload": """{"file":"file1-Wikipedia-Carbon.txt","url":"","vector_provider":"AI.OpenAI.OpenAITextEmbeddingGenerator","vector_generator":"TODO","last_update":"2023-12-26T21:25:26"}""",

          "content": """Carbon (from Latin carbo 'coal') is [REDACTED]  copper, which are weaker reducing agents at room temperature.""",

          "embedding": [-0.00891963486,0.0110242674,-0.0150581468, ...]"
        }
      },
      {
        "_index": "canimportonedocumentandaskasync",
        "_id": "d=doc001//p=d1f22975248a4c94bc7a9eabc868df57",
        "_score": 1,
        "_source": {
          "id": "d=doc001//p=d1f22975248a4c94bc7a9eabc868df57",
          "tags": [
            {
              "name": "__document_id",
              "value": "doc001"
            },
            {
              "name": "__file_type",
              "value": "text/plain"
            },
            {
              "name": "__file_id",
              "value": "0334bca3c83e442ea32f9cac85ff8763"
            },
            {
              "name": "__file_part",
              "value": "ac02b3efb75e47e3a3c17823ed44dbaf"
            },
            {
              "name": "indexedOn",
              "value": "2023-12-26T21:25:25.885+00:00"
            }
          ],

          "payload": """{"file":"file1-Wikipedia-Carbon.txt","url":"","vector_provider":"AI.OpenAI.OpenAITextEmbeddingGenerator","vector_generator":"TODO","last_update":"2023-12-26T21:25:26"}""",

          "content": """Carbon sublimes in a carbon arc, which has a temperature [REDACTED] hard tips for cutting tools.""",

          "embedding": [-0.00791963486,0.0120242674,-0.0160581468, ...]"
        }
      }
    ]
  }
}
```

These documents show how Kernel Memory information looks once it's indexed in Elasticsearch.

### How to pre-filter data before performing a kNN search

Assume we had a large number of documents in our index, and we wanted to limit the search to a subset of the data, for instance to documents that have a specific 
```__file_type``` such as ```text/plain```, or even just point to one document with ```__document_id``` = ```doc001```.

We can do that by adding a ```filter``` clause to the query, like the following:

```json
// Pre-filters the data by looking for documents that have 
// a tag with name '__document_id' and value 'doc001'.
// Then performs the kNN search on the filtered data.

GET default/_search
{
  "_source": {
    "excludes": ["embedding", "payload"]
  },
  "knn": {
    "filter": {
      "nested": {
      "path": "tags",
      "query": {
        "bool": {
          "must": [
            {
              "term": {
                "tags.name": {
                  "value": "__document_id"
                }
              }
            },
            {
              "match": {
                "tags.value": "doc001"
              }
            }
          ]
        }
      }
    }
    },
    "field": "embedding",
    "k": 10,
    "num_candidates": 100,
    "query_vector": [-0.00891963486,
      0.0110242674,
      -0.0150581468,
      -0.0392113142,
      -0.0102037108,
      ..]
```

In some situations, lexical search may perform better than semantic search. For example, when searching for single words or IDs, like the document type or document id.

The full documentation for Elasticsearch kNN query and examples can be found [here](https://www.elastic.co/guide/en/elasticsearch/reference/current/semantic-search.html#semantic-search-search).

## The connector to Elasticsearch

In order to implement a KM connector for [Elasticsearch](https://www.elastic.co/elasticsearch/), we need to implement the interface [IMemoryDb](https://github.com/microsoft/kernel-memory/blob/main/service/Abstractions/MemoryStorage/IMemoryDb.cs) and to be familiar with the related data structure [MemoryRecord](https://github.com/microsoft/kernel-memory/blob/main/service/Abstractions/MemoryStorage/MemoryRecord.cs). 

>*In the next article we will complete the connector by adding support for  [MemoryFilter](https://github.com/microsoft/kernel-memory/blob/main/service/Abstractions/Models/MemoryFilter.cs), which will allow the connector to (pre)filter datasets in multiple ways (i.e. key-based, full-text and semantic), and will enable powerful search that extend semantic search.*

The repository associated to this article is located [here](https://github.com/freemindlabsinc/FreeMindLabs.KernelMemory.Elasticsearch).




# What is IMemoryDb?

The IMemoryDb interface plays a pivotal role in Kernel Memory, serving as the gateway to store and retrieve vectors and payloads from the indices of a vector database or other storage systems capable of performing vector similarity searches.

This diagram shows the relationship between Kernel Memory and the IMemoryDb interface:

<div align="center">
  <img src="images/Connectors.jpg" width="50%" />
</div>

Kernel Memory can connect to any vector database or storage system if an IMemoryDb connector is available.

## The IMemoryDb interface

The [IMemoryDb](https://github.com/microsoft/kernel-memory/blob/main/service/Abstractions/MemoryStorage/IMemoryDb.cs) interface has seven methods, which can be thought as divided in three groups:

1. Index management
2. Data storage
3. Search

## Index management
These three methods allow to create, list and delete indices where we can store our data.

```csharp
Task CreateIndexAsync(string index, int vectorSize, CancellationToken ancellationToken)

Task<IEnumerable<string>> GetIndexesAsync(CancellationToken cancellationToken)

Task DeleteIndexAsync(string index, CancellationToken ancellationToken)
```

The methods are pretty self-explanatory, but here are some consideration on the arguments of those methods:

- **index**: is the name of the index/collection to create. The name is unique and can be case sensitive in some vector databases. Pay attention when picking names for your indices. Elasticsearch has some [restrictions on index names](https://www.elastic.co/guide/en/elasticsearch/reference/current/indices-create-index.html#indices-create-api-path-params).

- **vectorSize**: the size of the vectors to be stored in the index/collection. The size of the vectors is determined by the vectorization algorithm used to convert text into vectors. 
  - [OpenAI](https://www.openai.com) uses vectors with 1536 dimentiones.
  - [SBERT.net](https://sbert.net)'s all-MiniLM-L6-v2 uses vectors with 384 dimensions.

### Examples

The unit test [*IndexManagementTests.cs*](../tests/UnitTests/IndexManagementTests.cs) contains the test *CanCreateAndDeleteIndexAsync* which shows how to use ```IMemoryDb.CreateIndexAsync``` and ```IMemoryDb.DeleteIndexAsync```.

```csharp
[Fact]
public async Task CanCreateAndDeleteIndexAsync()
{
    var indexName = nameof(CanCreateAndDeleteIndexAsync);
    var vectorSize = 1536;

    // Creates the index using IMemoryDb
    await this.MemoryDb.CreateIndexAsync(indexName, vectorSize)
                       .ConfigureAwait(false);

    // Verifies the index is created using the ES client
    var actualIndexName = ESIndexName.Convert(indexName);
    var resp = await this.Client.Indices.ExistsAsync(actualIndexName)
                                        .ConfigureAwait(false);
    Assert.True(resp.Exists);
    this.Output.WriteLine($"The index '{actualIndexName}' was created successfully.");

    // Deletes the index
    await this.MemoryDb.DeleteIndexAsync(indexName)
                       .ConfigureAwait(false);

    // Verifies the index is deleted using the ES client
    resp = await this.Client.Indices.ExistsAsync(actualIndexName)
                                    .ConfigureAwait(false);
    Assert.False(resp.Exists);
    this.Output.WriteLine($"The index '{actualIndexName}' was deleted successfully.");
}
```

The unit test also contains the test *CanGetIndicesAsync* which shows how to use ```IMemoryDb.GetIndexesAsync```.

```csharp
[Fact]
    public async Task CanGetIndicesAsync()
    {
        var indexNames = new[]
        {
            $"{nameof(CanGetIndicesAsync)}-First",
            $"{nameof(CanGetIndicesAsync)}-Second"
        };

        // Creates the indices using IMemoryDb
        foreach (var indexName in indexNames)
        {
            await this.MemoryDb.CreateIndexAsync(indexName, 1536)
                               .ConfigureAwait(false);
        }

        // Verifies the indices are returned
        var indices = await this.MemoryDb.GetIndexesAsync()
                                         .ConfigureAwait(false);

        Assert.True(indices.All(nme => indices.Contains(nme)));

        // Cleans up
        foreach (var indexName in indexNames)
        {
            await this.MemoryDb.DeleteIndexAsync(indexName)
                               .ConfigureAwait(false);
        }
    }
```

#### Considerations on index management methods

Here are some possible improvements:

4. GetIndexesAsync could return an ```IAsyncEnumerable<string>``` instead of an ```IEnumerable<string>```. This would allow to return the list of indices in batches, which would be useful when there are a large number of indices.

5. It would be nice if we could have some filters (wildcard at least) in GetIndexesAsync, so that we can filter the list of indices by name (e.g. 'testIndices-*')

## Data storage

The next two methods allow to update and delete the information inside indices using the data structure [MemoryRecord](https://github.com/microsoft/kernel-memory/blob/main/service/Abstractions/MemoryStorage/MemoryRecord.cs). The methods are:
```csharp
Task<string> UpsertAsync(string index, MemoryRecord record, CancellationToken cancellationToken = default(CancellationToken));

Task DeleteAsync(string index, MemoryRecord record, CancellationToken cancellationToken = default(CancellationToken));
```

Also these 2 methods are pretty self-explanatory, but before we see how to use them, we need to understand what a MemoryRecord is.

### MemoryRecord

MemoryRecord is a class that helps us to store and retrieve information from a vector database. To store a document in a vector database, the document first needs to be partitioned in smaller chunks of text. Each chunk is then converted into a vector and stored in the vector database separately.

> You can see a full example of how this is done in the method *UpsertTextFilesAsync* of the unit test [DataStorageTests.cs](../tests/UnitTests/DataStorageTests.cs).

So, if a document is large than the configured  ends up being split in 3 partitions/chunks, we will have 3 separate Elasticseach documents with the same document id(```d```) but 3 different ```p``` parts similar to ```d=doc001//p=904d05afeeb24c2b99c9ad73fd97e690```

MemoryRecord It is declared as follows:

```csharp
public class MemoryRecord
{    
    public string Id { get; set; } = string.Empty;

    public Embedding Vector { get; set; } = new();

    public TagCollection Tags { get; set; } = new();

    public Dictionary<string, object> Payload { get; set; } = new();
}
```

- The property ```Id``` is a unique identifier for the record. Ids are currently formatted like ```d=doc001//p=904d05afeeb24c2b99c9ad73fd97e690``` where the ```d``` part is the document id and the ```p``` part is the id of the document partition (chunk). 


- The property ```Vector``` is the vector to be stored in the index. It's essentially an array of floats.

- The property ```Tags``` is a collection of key-value pairs that can be used to filter the data when performing searches. Multiple values per keys are supported (e.g. ```"Collection=Work", "Project=1", "Project=2", "Project=3", "Type=Chat", "LLM=AzureAda2"```). 
  - Uses cases include 
    - Collections, e.g. [ "Collection=Project1", "Collection=Work" ]
    - Folders, e.g. [ "Folder=Inbox", "Folder=Spam" ]
    - Content types, e.g. [ "Type=Chat" ]
    - Versioning, e.g. [ "LLM=AzureAda2", "Schema=1.0" ]
    - etc.   

- The property ```Payload``` is a dictionary of key-value pairs that can be used to store additional information about the record. The payload is not searchable, but it can be retrieved when the record is returned from a search. 
    - Use cases include 
        - Citations
        - Original text
        - Descriptions
        - Embedding generator name
        - URLs
        - Content type
        - Timestamps
        - etc.

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

```
{
  "your_index_name_": {
    "mappings": {
      "properties": {
        "content": {
          "type": "text"
        },
        "embedding": {
          "type": "dense_vector",
          "dims": 1536,
          "index": true,
          "similarity": "cosine"
        },
        "id": {
          "type": "keyword"
        },
        "payload": {
          "type": "text",
          "index": false
        },
        "tags": {
          "type": "nested",
          "properties": {
            "name": {
              "type": "keyword"
            },
            "value": {
              "type": "text"
            }
          }
        }
      }
    }
  }
}
```

And this is the data stored in one of such memory record:

```
{
    "_index": "your_index_name",
    "_id": "d=doc001//p=904d05afeeb24c2b99c9ad73fd97e690",
    "_score": 1,
    "_source": {
      
      "id": "d=doc001//p=904d05afeeb24c2b99c9ad73fd97e690",

      "tags": [
        {
          "name": "__document_id",
          "value": "doc001"
        },
        {
          "name": "__file_type",
          "value": "text/plain"
        },
        {
          "name": "__file_id",
          "value": "0334bca3c83e442ea32f9cac85ff8763"
        },
        {
          "name": "__file_part",
          "value": "d414c1d106524bb2a6eddf51d5b92e18"
        },
        {
          "name": "indexedOn",
          "value": "2023-12-26T21:25:25.885+00:00"
        }
      ],
      
      // ** PAYLOAD ** 
      "payload": """{"file":"file1-Wikipedia-    arbon.txt","url":"","vector_provider":"AI.OpenAI.OpenAITextEmbeddingGenerator","vector_generator":"TODO","last_update":"2023-12-26T21:25:6"}""" ,
    
      "content": """Carbon (from Latin carbo 'coal') is [REDACTED]  copper, which are weaker reducing agents at room temperature.""",
    
      "embedding": [-0.00891963486,0.0110242674,-0.0150581468, ...]"
    }      
}
```

What we store and search in vector databases are collections of MemoryRecords.
These collection tend to become populated with a large number of records, and they are often sharded across multiple nodes.

### UpsertAsync and DeleteAsync

The methods to insert-update and delete MemoryRecords are defined as follows:

```csharp
Task<string> UpsertAsync(string index, MemoryRecord record, CancellationToken ancellationToken)

Task DeleteAsync(string index, MemoryRecord record, CancellationToken ancellationToken)
```

Here are some considerations:

1. There is no distinction between inserting and updating a record: ```Upsert``` does both things:
    - If the record exists, it will be updated. 
      - The method should return the same value of ```record.Id``` of the submitted record.
    - If it doesn't exist, it will be inserted and a new id will be generated and returned.
      - [TO VERIFY] what happens to record.id if its null and we upsert?
        - Q1: Does it generate a new id and return it? (preferrable)
          - Does record.id get updated like Entity Framework would do?
        - Q2: Or does it expect the new ```record.Id``` to be populated by the client?             

Possible improvements:

1. Upsert and DeleteAsync should be able to accept a list of MemoryRecords to be inserted/updated/deleted. This would allow to perform batch operations and improve performance.

2. It would be beneficial to have methods like ES's DeleteByQuery to speed up deletions.

3. There should be a Delete that just takes the index and the id(s) of the records to be deleted. This would be useful to delete records by id without having to create a MemoryRecord object.

> TODO: put implementation code for methods here
> TODO: put code of CRUD tests here

### Considerations on ids and performance 

Pay special consideration to the ```Id``` property of MemoryRecord.
Having ids that are balanced across shards is important for performance reasons.

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
The Results of a search are not returned all at once, but rather in batches, ordered by a relevance score.

  - The caller can then asynchronously iterate over the results as they are returned.

  - The caller can also stop the search at any time, and only get back the results that have been returned so far.

Possible improvements:

1. I would simply make the ```text``` p[roperty of the first overload to be nullable.
  - If a ```null``` value is passed in ```text```, then the search will be performed without a text query.


> TODO: put implementation of search here
> TODO: put code of sample for semantic search here

## The complete implementation of the Elasticsearch connector

> show [code here](https://github.com/freemindlabsinc/FreeMindLabs.SemanticKernel/blob/main/src/ElasticsearchMemoryStorage/ElasticsearchMemory.cs)

> TODO: make sure URL is good after changing repo name.

## Where do we go from here?

>explain that we will add support for filters in the next article bla bla

In the next article we will complete the connector by adding support for  [MemoryFilter](https://github.com/microsoft/kernel-memory/blob/main/service/Abstractions/Models/MemoryFilter.cs), and we will see how we cann even extend this and other classes to support more complex filters.
