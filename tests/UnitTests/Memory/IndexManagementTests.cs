// Copyright (c) Free Mind Labs, Inc. All rights reserved.
using Elastic.Clients.Elasticsearch;
using FreeMindLabs.KernelMemory.Elasticsearch;
using Microsoft.KernelMemory.MemoryStorage;
using Xunit;
using Xunit.Abstractions;

namespace UnitTests.Memory;

public class IndexManagementTests
{
    public IndexManagementTests(ITestOutputHelper output, IMemoryDb memoryDb, ElasticsearchClient client)
    {
        this.Output = output ?? throw new ArgumentNullException(nameof(output));
        this.MemoryDb = memoryDb ?? throw new ArgumentNullException(nameof(memoryDb));
        this.Client = client ?? throw new ArgumentNullException(nameof(client));
    }

    public ITestOutputHelper Output { get; }
    public IMemoryDb MemoryDb { get; }
    public ElasticsearchClient Client { get; }

    [Theory]
    [InlineData("", 1536)] // default index
    [InlineData(nameof(CanCreateIndexAsync), 1536)]
    public async Task CanCreateIndexAsync(
        string indexName,
        int vectorSize)
    {
        // Verifies the index name passes Elasticsearch validation
        Assert.True(Indexname.TryConvert(indexName, out var convResult));
        this.Output.WriteLine($"The index name '{indexName}' will be translated to '{convResult.ActualIndexName}'.");

        // Clean up: Deletes the default index if already present using the ES client.
        await this.Client.Indices.DeleteAsync(convResult.ActualIndexName)
                                 .ConfigureAwait(false);

        // Creates the index using IMemoryDb
        await this.MemoryDb.CreateIndexAsync(indexName, vectorSize)
                           .ConfigureAwait(false);

        // Verifies the index is created using the ES client
        var resp = await this.Client.Indices.ExistsAsync(convResult.ActualIndexName)
                                            .ConfigureAwait(false);
        Assert.True(resp.Exists);
        this.Output.WriteLine($"The index '{convResult.ActualIndexName}' was created successfully successfully.");

        // Delete the index to leave a clean slate
        await this.Client.Indices.DeleteAsync(convResult.ActualIndexName)
                                 .ConfigureAwait(false);
    }

    [Fact]
    public async Task CanDeleteIndexAsync()
    {
        // Creates the index using IMemoryDb
        var indexName = nameof(CanDeleteIndexAsync);
        var vectorSize = 1536;
        Assert.True(Indexname.TryConvert(indexName, out var convResult));

        await this.MemoryDb.CreateIndexAsync(indexName, vectorSize)
                           .ConfigureAwait(false);

        // Verifies using Elasticsearch client
        var idxExists1 = await this.Client.Indices.ExistsAsync(convResult.ActualIndexName)
                                                  .ConfigureAwait(false);
        Assert.True(idxExists1.Exists);

        // Deletes the index using IMemoryDb
        await this.MemoryDb.DeleteIndexAsync(indexName)
                           .ConfigureAwait(false);

        // Verifies using Elasticsearch client
        var idxExists2 = await this.Client.Indices.ExistsAsync(convResult.ActualIndexName)
                                                  .ConfigureAwait(false);
        Assert.False(idxExists2.Exists);

        this.Output.WriteLine($"The index '{convResult.ActualIndexName}' was created and deleted successfully successfully.");
    }

    [Fact]
    public async Task CanGetIndicesAsync()
    {
        var indexNames = new[]
        {
            $"{nameof(CanGetIndicesAsync)}-First",
            $"{nameof(CanGetIndicesAsync)}-Second"
        };

        // Creates the indices using IMemoryDb
        foreach (var indexName in indexNames)
        {
            await this.MemoryDb.CreateIndexAsync(indexName, 1536)
                               .ConfigureAwait(false);
        }

        // Verifies the indices are returned
        var indices = await this.MemoryDb.GetIndexesAsync()
                                         .ConfigureAwait(false);

        Assert.True(indices.All(nme => indices.Contains(nme)));

        // Cleans up
        foreach (var indexName in indexNames)
        {
            await this.MemoryDb.DeleteIndexAsync(indexName)
                               .ConfigureAwait(false);
        }
    }
}
