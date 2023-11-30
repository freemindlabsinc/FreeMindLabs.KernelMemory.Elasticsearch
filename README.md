# Elasticsearch Memory Storage 
**By Free Mind Labs, Inc.** 
*A proper webpage is coming soon...* :blush:)

[![License: MIT](https://img.shields.io/github/license/microsoft/kernel-memory)](https://github.com/freemindlabsinc/FreeMindLabs.SemanticKernel/blob/main/LICENSE)
Use [Elasticsearch](https://www.elastic.co/) as vector storage for [Microsoft](https://www.microsoft.ms) **[Kernel Memory](https://github.com/microsoft/semantic-memory)** (KM)

>*KM is an open-source service and plugin specialized in the efficient indexing of datasets through custom continuous data hybrid pipelines.*

This repository contains the Elasticsearch adapter allowing to use Kernel Memory with Elasticsearch.

## Goals
1. To implement an Elasticsearch [IVectorDb](https://github.com/microsoft/kernel-memory/blob/ea157ef2b837e2cd40165dc9f6a578a2e98bd3e3/service/Core/MemoryStorage/IVectorDb.cs#L9) 	
    1. This interface might be soon renamed...
	1. This allows to use Elasticsearch as vector database directly from Kernel Memory.
	1. KM can also be used as IMemoryStore for SK

## Current status
1. The connector is currently in development and not ready for production (or any) use.
1. The connector is not yet available as a NuGet package.

## Pre-requisites
1. A running instance of Elasticsearch
1. User Secrets configured as explained in [Configuration](CONFIGURATION.md)
1. TBC



## Challenges
The new API in the Elasticsearch client is not yet feature complete and there are bugs.

1. AutoMap(),etc. missing
    1. [CreateIndexDescriptor Mappings -> Map Api usage issue #7929](https://github.com/elastic/elasticsearch-net/issues/7929)
    1. [FEATURE - Support AutoMap to allow creation of mappings using type inference #6610](https://github.com/elastic/elasticsearch-net/issues/6610)
        1. flobernd comment on Aug 17 suggests this:
```
var mapResponse = client.Indices.PutMapping("index", x => x
    .Properties<Person>(p => p
        .DenseVector(x => x.Data, d => d
            .Index(true)
            .Similarity("dot_product"))));
```


## Resources

1. Elastic's official docs on the client.
    1. NEST 7.17: https://www.elastic.co/guide/en/elasticsearch/client/net-api/7.17/nest-getting-started.html
    1. New client 8.9: https://www.elastic.co/guide/en/elasticsearch/client/net-api/8.9/introduction.html
        1. This client is not yet feature complete.
            1. Look here for details: https://www.elastic.co/guide/en/elasticsearch/client/net-api/current/release-notes-8.0.0.html
        1. In addition, the docs are not up to date. For some stuff we need to lok at NEST's docs.

1. [Elasticsearch.net Github repository](https://github.com/elastic/elasticsearch-net)    


1. Semantic Kernel/Memory-Kernel
    1. [Introduction to Semantic Memory (feat. Devis Lucato) | Semantic Kernel](https://www.youtube.com/watch?v=5JYW_uAxwYM)
    1. [11.29.2023 - Semantic Kernel Office Hours (US/Europe Region)](https://www.youtube.com/watch?v=JSca9mVUUJo)