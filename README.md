# Kernel Memory with Elasticsearch

Use [Elasticsearch](https://www.elastic.co/) as vector storage for Microsoft [Kernel Memory](https://github.com/microsoft/semantic-memory).

[![NuGet](https://img.shields.io/nuget/v/Freemindlabs.KernelMemory.Elasticsearch.svg)](https://www.nuget.org/packages/Freemindlabs.KernelMemory.Elasticsearch) [![NuGet](https://img.shields.io/nuget/dt/Freemindlabs.KernelMemory.Elasticsearch.svg)](https://www.nuget.org/packages/Freemindlabs.KernelMemory.Elasticsearch) [![License: MIT](https://img.shields.io/github/license/microsoft/kernel-memory)](https://github.com/freemindlabsinc/FreeMindLabs.SemanticKernel/blob/main/LICENSE) 


**Kernel Memory** (KM) is a **multi-modal [AI Service](service/Service/README.md)** specialized in the efficient indexing of datasets through custom continuous data hybrid pipelines, with support for **[Retrieval Augmented eneration](https://en.wikipedia.org/wiki/Prompt_engineering#Retrieval-augmented_generation)** (RAG), synthetic memory, prompt engineering, and custom semantic memory processing.

<img src="content/images/Pipelines.jpg"/>

Utilizing advanced embeddings and LLMs, the system enables Natural Language querying for obtaining answers from the indexed data, complete with citations and links to the original sources.

<img src="content/images/RAG.jpg"/>

---

This repository contains the **Elasticsearch adapter** that allows KM to use Elasticsearch as vector database, thus allowing developers to perform [lexical and semantic search](https://www.elastic.co/search-labs/blog/articles/lexical-and-semantic-search-with-elasticsearch), in addition to hybrid, keyword and full-text search.

>If you want to read more about semantic search, click [here](https://www.elastic.co/guide/en/elasticsearch/reference/current/semantic-search.html). For more on hybrid search, click [here](https://opster.com/guides/elasticsearch/machine-learning/elasticsearch-hybrid-search/)

## Pre-requisites

1. A running instance of Elasticsearch    
    1. You can install a local instance of Elasticsearch using Docker. See *[How to install the Elastic Stack using Docker Compose](/docker/README2.md)*.
  1. 

## Goals

1. To implement an maintain an open source Elasticsearch [IMemoryDB](https://github.com/microsoft/kernel-memory/blob/adce865a472728f2549428cf6b82ca79a601582b/service/Abstractions/MemoryStorage/IMemoryDb.cs#L9) connector for Kernel Memory.

    1. Free Mind Labs require such connector to complete [Videomatic](https://github.com/freemindlabsinc/videomatic).
        
    1. The basic connector (i.e. the complete implementation of IMemoryDb) will be free of charge and open source.

2. In the future we hope to add additional features (e.g. *advanced search options for pre and post filtering, analytics, ES-specific features, etc.*) that could generate some revenue to support this and other projects. 
    1. Patreon?
    1. GitHub donations?
    1. Other?

*We'd love to hear what you think about this.* 

## Updates

- :notebook: This repo is growing and right now we log our thoughts and progress in  [this diary page](DIARY.md).
- :hammer_and_wrench: An additional list of TODOs can be found [here](TODO.md).

## How to setup a running instance of Elasticsearch

You need to have [Docker](https://docs.docker.com/get-docker/) and [Docker Compose](https://docs.docker.com/compose/install/) installed before continuing.

To simplify the setup of a running instance of Elasticsearch we created the article [Installing the Elastic Stack using Docker Compose](/docker/Readme.md) that guides you through the process.  It is a three step operation and it should take less than five minutes to complete.

The following diagram shows what we will be running once we complete the installation.

<div align="center">
    <img src="docker/images/ELKStack.png" width="500px"</img>
</div>

More details about the individual components in the diagram can be found in [the installation article](/docker/Readme.md).

## The .NET Solution

This is a screenshot of the solution. 
We highlighted some of the most important files for you to explore and look at.

<p align="center">
    <img src="content/images/Solution.png" width=500 />
</p>

---

Here are some screenshots of the tests included in the project. This project uses a test-first approach, so you should expect to find examples of how to use the connector in the tests themselves.

:warning: Future articles will explain in better detail what the tests do.

>

Look at the output window to see what they do.

<p align="center">
 <img src="/content/images/BehavesLike.jpg" width=500 />
</p>

Click [here](tests/UnitTests/MemoryStorage/MemoryStorageTests.cs) to see the source code of the test.

<p align="center">
 <img src="/content/images/CarbonBondTo.jpg" width=500 />
</p>

Click [here](tests/UnitTests/Serverless/ServerlessTest.cs) to see the source code of the test.

### Mappings
The examples use the OpenAI's text-embedding-ada-002.
It is possible to use any other embedding model supported by SK (e.g. Azure Open AI and Hugging Face).

<p align="center">
 <img src="/content/images/Mappings.jpg" width=900 />
</p>

### Kibana
Here are some screenshots of the data stored in ES, after running the tests in the solution.

<p align="center">
 <img src="/content/images/DataPageAllRows.jpg" width=600 />
</p>

<p align="center">
 <img src="/content/images/DataPage1.jpg" width=600 />
</p>

<p align="center">
 <img src="/content/images/DataPage2.jpg" width=600 />
</p>

### KNN Query
Here's an example of how to run semantic search directly on ES.

<p align="center">
 <img src="/content/images/KnnQuery.jpg" width=600 />
</p>


## Resources

1. [A Quick Introduction to Vector Search](https://opster.com/guides/opensearch/opensearch-machine-learning/introduction-to-vector-search/)
1. [Elasticsearch Hybrid Search](https://opster.com/guides/elasticsearch/machine-learning/elasticsearch-hybrid-search/)

1. Elastic's official docs on the client.
    1. NEST 7.17: https://www.elastic.co/guide/en/elasticsearch/client/net-api/7.17/nest-getting-started.html
    1. New client 8.9: https://www.elastic.co/guide/en/elasticsearch/client/net-api/8.9/introduction.html
        1. This client is not yet feature complete.
            1. Look here for details: https://www.elastic.co/guide/en/elasticsearch/client/net-api/current/release-notes-8.0.0.html
        1. In addition, the docs are not up to date. For some stuff we need to lok at NEST's docs.

1. [Elasticsearch.net GitHub repository](https://github.com/elastic/elasticsearch-net)

1. Semantic Kernel/Memory-Kernel
    1. [Introduction to Semantic Memory (feat. Devis Lucato) | Semantic Kernel](https://www.youtube.com/watch?v=5JYW_uAxwYM)
    1. [11.29.2023 - Semantic Kernel Office Hours (US/Europe Region)](https://www.youtube.com/watch?v=JSca9mVUUJo)   