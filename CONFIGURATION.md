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

**Notice how the UserSecretsId of the test project is set to the same value of Semantic Kernel and Kernel Memory:** 5ee045b0-....-....-....-32d1a6f8fed0
*By virtue of doing this we can use the same secrets file for all projects.*

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