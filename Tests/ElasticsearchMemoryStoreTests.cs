using Elastic.Clients.Elasticsearch;
using Elastic.Transport;
using FreeMindLabs.SemanticKernel.Connectors.Elasticsearch;
using Microsoft.Extensions.Configuration;

namespace Tests;

/// <summary>
/// The tests for <see cref="ElasticsearchMemoryStore"/>.
/// </summary>
public class ElasticsearchMemoryStoreTests
{
    /// <summary>
    /// Create a collection in ES.
    /// </summary>
    [Fact]
    public async Task ShouldCreateCollection()
    {
        var cfg = new ConfigurationBuilder()
            .AddUserSecrets<ElasticsearchMemoryStoreTests>()
            .Build();

        using var settings = new ElasticsearchClientSettings(new Uri(cfg["Elasticsearch:Url"]));

        var auth = new BasicAuthentication(cfg["Elasticsearch:UserName"], cfg["Elasticsearch:Password"]);
        settings.Authentication(auth)
            .ServerCertificateValidationCallback((sender, certificate, chain, errors) => true);
              //.CertificateFingerprint(cfg["Elasticsearch:CertificateFingerPrint"]);            

        var ms = new ElasticsearchMemoryStore(settings);
        await ms.CreateCollectionAsync("test")
                .ConfigureAwait(false);
    }
}