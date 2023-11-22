# FreeMindLabs.SemanticKernel.Connectors.Elasticsearch
Microsoft [Semantic Kernel](https://github.com/microsoft/semantic-kernel) and [Kernel Memory](https://github.com/microsoft/kernel-memory/tree/ea157ef2b837e2cd40165dc9f6a578a2e98bd3e3) connectors to use Elasticsearch as vector database.

## Goals
1. To implement an Elasticsearch [IMemoryStore](https://github.com/microsoft/semantic-kernel/blob/c883d78d2a4ddf6142cac1b149aacc085d0874e4/dotnet/src/SemanticKernel.Abstractions/Memory/IMemoryStore.cs#L13) 
	1. This allows to use Elasticsearch as vector database directly from Semantic Kernel.
1. To implement an Elasticsearch [IVectorDb](https://github.com/microsoft/kernel-memory/blob/ea157ef2b837e2cd40165dc9f6a578a2e98bd3e3/service/Core/MemoryStorage/IVectorDb.cs#L9) 	
	1. This allows to use Elasticsearch as vector database directly from Kernel Memory.	
	1. *KM can also be used as IMemoryStore for SK**

## License
Copyright (c) Free Mind Labs, Inc. All rights reserved.

Licensed under the Apache 2.0 license.