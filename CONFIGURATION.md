# Configuration
The xUnit project UnitTests contains an [appSettings.json](tests/UnitTests/appSettings.json) file that lists all available options. The file reads as follows:

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
This file is supposed to show the available options but it is not meant to store sensitive information. 
Modify it as necessary (e.g. by changing the Endpoint), but add the values for the certificate fingerprint and the password in user secrets.

>*The class used to store configuration is [ElasticsearchConfig](/src/ElasticsearchMemoryStorage/ElasticsearchConfig.cs).*

## User Secrets

First, notice how the UserSecretsId of the test project is set to the same value of Semantic Kernel and Kernel Memory:
```
<UserSecretsId>5ee045b0-aea3-4f08-8d31-32d1a6f8fed0</UserSecretsId>
```
By virtue of doing this we can use the **same secrets file for all the projects** in SK, KM and these projects.

### How to add user secrets

To add secrets either:
1. Open the secrets file in your IDE by right clicking on the project name and selecting Manage User Secrets.
    - To read more about user secrets click [here](https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets?view=aspnetcore-8.0&tabs=windows)

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