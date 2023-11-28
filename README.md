# FreeMindLabs.SemanticKernel.Connectors.Elasticsearch
Microsoft [Semantic Kernel](https://github.com/microsoft/semantic-kernel) and [Kernel Memory](https://github.com/microsoft/kernel-memory/tree/ea157ef2b837e2cd40165dc9f6a578a2e98bd3e3) connectors to use Elasticsearch as vector database.

## Goals
1. To implement an Elasticsearch [IVectorDb](https://github.com/microsoft/kernel-memory/blob/ea157ef2b837e2cd40165dc9f6a578a2e98bd3e3/service/Core/MemoryStorage/IVectorDb.cs#L9) 	
	1. This allows to use Elasticsearch as vector database directly from Kernel Memory.
	1. KM can also be used as IMemoryStore for SK

## Current status
1. The connector is currently in development and not ready for production use.
1. The connector is not yet available as a NuGet package.

## Pre-requisites
1. A running instance of Elasticsearch
1. TBC

## Configuration
The xUnit project UnitTests contains an - *appSettings.json* - that lists all available options.

The file is located at the root of the Tests project and it reads as follows:

```
{
  "Elasticsearch": {
    "CertificateFingerPrint": "...SECRETS...",
    "Endpoint": "http://localhost:9200",
    "UserName": "...SECRETS...",
    "Password": "...SECRETS..."
  }
}
```

**Modify it as necessary and add the values for the certificate fingerprint and the password in User Secrets.**

> Notice how the UserSecretsId of the test project is set to the same value of Semantic Kernel and Kernel Memory: 5ee045b0-....-....-....-32d1a6f8fed0
> By virtue of doing this we can use the same secrets file for all projects.


To add the Elasticsearch secrets either:
1. Open the secrets file in your IDE 
1. Add the secrets from the command line by running the following commands:
```
> dotnet user-secrets set "Elasticsearch:CertificateFingerPrint" "...your value..."
> dotnet user-secrets set "Elasticsearch:UserName" "...your value..."
> dotnet user-secrets set "Elasticsearch:Password" "...your value..."
```


This ultimately results in the following secrets.json additions:
```
{  
  [..]
  "Elasticsearch:CertificateFingerPrint": "...your value...",
  "Elasticsearch:UserName": "...your value...",
  "Elasticsearch:Password": "...your value...",  
}
```





## License
Copyright (c) Free Mind Labs, Inc. All rights reserved.

Licensed under the Apache 2.0 license.