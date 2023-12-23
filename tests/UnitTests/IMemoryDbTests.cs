// Copyright (c) Free Mind Labs, Inc. All rights reserved.
using Elastic.Clients.Elasticsearch;
using FreeMindLabs.KernelMemory.Elasticsearch;
using Microsoft.KernelMemory.MemoryStorage;
using Xunit;
using Xunit.Abstractions;
using Xunit.DependencyInjection;

namespace UnitTests;

public class IMemoryDbTests
{
    private readonly ITestOutputHelper _output;

    public IMemoryDbTests(ITestOutputHelper output)
    {
        this._output = output ?? throw new ArgumentNullException(nameof(output));
    }

    [Theory]
    [InlineData("", 1536, null)] // default index
    [InlineData("nondefault", 1536, null)]
    [InlineData("WithUppercase", 1536, null)]
    [InlineData("With-Dashes", 1536, null)]
    [InlineData("123numberfirst", 1536, null)]
    public async Task CanCreateIndexAsync(
        string indexName,
        int vectorSize,
        [FromServices] IMemoryDb memory,
        [FromServices] ElasticsearchClient client)
    {
        ArgumentNullException.ThrowIfNull(memory);
        ArgumentNullException.ThrowIfNull(client);

        // Verifies the index name passes Elasticsearch validation
        Assert.True(Indexname.TryConvert(indexName, out var convResult));
        this._output.WriteLine($"The index name '{indexName}' will be translated to '{convResult.ActualIndexName}'.");

        // Clean up: Deletes the default index if already present using the ES client.
        await client.Indices.DeleteAsync(convResult.ActualIndexName)
                            .ConfigureAwait(false);

        // Creates the index using IMemoryDb
        await memory.CreateIndexAsync(indexName, vectorSize)
                    .ConfigureAwait(false);

        // Verifies the index is created using the ES client
        var resp = await client.Indices.ExistsAsync(convResult.ActualIndexName)
                                       .ConfigureAwait(false);
        Assert.True(resp.Exists);
        this._output.WriteLine($"The index '{convResult.ActualIndexName}' was created successfully successfully.");

        // Delete the index to leave a clean slate
        await client.Indices.DeleteAsync(convResult.ActualIndexName)
                            .ConfigureAwait(false);
    }
}
