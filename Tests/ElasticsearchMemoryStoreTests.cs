using FreeMindLabs.SemanticKernel.Connectors.Elasticsearch;

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
    public void ShouldCreateCollection()
    {
        var ms = new ElasticsearchMemoryStore();
        ms.CreateCollectionAsync("test");
    }
}