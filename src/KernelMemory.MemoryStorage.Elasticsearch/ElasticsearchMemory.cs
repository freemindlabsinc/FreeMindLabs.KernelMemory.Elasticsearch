using Microsoft.KernelMemory;
using Microsoft.KernelMemory.MemoryStorage;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace FreeMindLabs.KernelMemory.MemoryStorage.Elasticsearch;

/// <summary>
/// xxx
/// </summary>
public class ElasticsearchMemory : IVectorDb
{
    public ElasticsearchMemory()
    {
            
    }

    /// <inheritdoc/>
    public Task CreateIndexAsync(string index, int vectorSize, CancellationToken cancellationToken = default)
    {
        throw new System.NotImplementedException();
    }

    /// <inheritdoc/>
    public Task DeleteAsync(string index, MemoryRecord record, CancellationToken cancellationToken = default)
    {
        throw new System.NotImplementedException();
    }

    /// <inheritdoc/>
    public Task DeleteIndexAsync(string index, CancellationToken cancellationToken = default)
    {
        throw new System.NotImplementedException();
    }

    /// <inheritdoc/>
    public Task<IEnumerable<string>> GetIndexesAsync(CancellationToken cancellationToken = default)
    {
        throw new System.NotImplementedException();
    }

    /// <inheritdoc/>
    public IAsyncEnumerable<MemoryRecord> GetListAsync(string index, ICollection<MemoryFilter>? filters = null, int limit = 1, bool withEmbeddings = false, CancellationToken cancellationToken = default)
    {
        throw new System.NotImplementedException();
    }

    /// <inheritdoc/>
    public IAsyncEnumerable<(MemoryRecord, double)> GetSimilarListAsync(string index, Embedding embedding, ICollection<MemoryFilter>? filters = null, double minRelevance = 0, int limit = 1, bool withEmbeddings = false, CancellationToken cancellationToken = default)
    {
        throw new System.NotImplementedException();
    }

    /// <inheritdoc/>
    public Task<string> UpsertAsync(string index, MemoryRecord record, CancellationToken cancellationToken = default)
    {
        throw new System.NotImplementedException();
    }
}
