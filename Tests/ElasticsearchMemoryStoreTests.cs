using Elastic.Clients.Elasticsearch;
using Elastic.Transport;
using FreeMindLabs.SemanticKernel.Connectors.Elasticsearch;
using Microsoft.Extensions.Configuration;
using Microsoft.SemanticKernel.Memory;

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
        var ms = GetMemoryStore();

        await ms.CreateCollectionAsync("test")
                .ConfigureAwait(false);
    }

    #region To move elsewhere/refactor

    static private IMemoryStore GetMemoryStore()
    {
        IConfigurationRoot cfg = GetConfiguration();

        // TODO: IDisposable is weird...
        using var settings = new ElasticsearchClientSettings(new Uri(cfg["Elasticsearch:Url"]));

        settings.Authentication(new BasicAuthentication(cfg["Elasticsearch:UserName"], cfg["Elasticsearch:Password"]))
            // TODO: Not sure why I need this. Verify configuration maybe?
            .ServerCertificateValidationCallback((sender, certificate, chain, errors) => true)
            .CertificateFingerprint(cfg["Elasticsearch:CertificateFingerPrint"]);

        IMemoryStore ms = new ElasticsearchMemoryStore(settings);
        return ms;
    }

    private static IConfigurationRoot GetConfiguration()
    {
        return new ConfigurationBuilder()
                    .AddJsonFile("testSettings.json")
                    .AddUserSecrets<ElasticsearchMemoryStoreTests>()
                    .Build();
    }

    #endregion
}