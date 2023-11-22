using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.IndexManagement;
using Microsoft.SemanticKernel.Memory;

namespace FreeMindLabs.SemanticKernel.Connectors.Elasticsearch;

/// <summary>
/// Elasticsearch implementation of <see cref="IMemoryStore"/>.
/// </summary>
public class ElasticsearchMemoryStore : IMemoryStore
{    
    /// <summary>
    /// Creates a memory store using the specified Elasticsearch <paramref name="settings"/>.
    /// </summary>
    /// <param name="settings"></param>
    public ElasticsearchMemoryStore(IElasticsearchClientSettings settings)
    {
        Client = new ElasticsearchClient(settings);
    }

    ElasticsearchClient Client { get; }

    /// <inheritdoc />
    public Task CreateCollectionAsync(string collectionName, CancellationToken cancellationToken = default)
    {
        return Client.Indices.CreateAsync(collectionName, cancellationToken: cancellationToken);
    }

    /// <inheritdoc />
    public Task DeleteCollectionAsync(string collectionName, CancellationToken cancellationToken = default)
    {
        return Client.Indices.DeleteAsync(collectionName, cancellationToken: cancellationToken);
    }

    /// <inheritdoc />
    public async Task<bool> DoesCollectionExistAsync(string collectionName, CancellationToken cancellationToken = default)
    {
        var response = await Client.Indices
            .ExistsAsync(collectionName, cancellationToken: cancellationToken)
            .ConfigureAwait(false); // TODO: verify this is correct
        return response.Exists;
    }

    /// <inheritdoc />
    public Task<MemoryRecord?> GetAsync(string collectionName, string key, bool withEmbedding = false, CancellationToken cancellationToken = default)
    {

        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public IAsyncEnumerable<MemoryRecord> GetBatchAsync(string collectionName, IEnumerable<string> keys, bool withEmbeddings = false, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public IAsyncEnumerable<string> GetCollectionsAsync(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public Task<(MemoryRecord, double)?> GetNearestMatchAsync(string collectionName, ReadOnlyMemory<float> embedding, double minRelevanceScore = 0, bool withEmbedding = false, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public IAsyncEnumerable<(MemoryRecord, double)> GetNearestMatchesAsync(string collectionName, ReadOnlyMemory<float> embedding, int limit, double minRelevanceScore = 0, bool withEmbeddings = false, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public Task RemoveAsync(string collectionName, string key, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public Task RemoveBatchAsync(string collectionName, IEnumerable<string> keys, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public Task<string> UpsertAsync(string collectionName, MemoryRecord record, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public IAsyncEnumerable<string> UpsertBatchAsync(string collectionName, IEnumerable<MemoryRecord> records, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}