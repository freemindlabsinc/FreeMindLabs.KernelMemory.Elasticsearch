// Copyright (c) Free Mind Labs, Inc. All rights reserved.
using Elastic.Clients.Elasticsearch;
using FreeMindLabs.KernelMemory.Elasticsearch;
using FreeMindLabs.KernelMemory.Elasticsearch.Exceptions;
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
        Assert.True(ElasticsearchIndexname.TryConvert(indexName, out var convResult));
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

    [Theory]
    // An index name cannot start with a hyphen (-) or underscore (_).
    [InlineData("-test", 1)]
    [InlineData("test-", 1)]
    [InlineData("_test", 1)]
    [InlineData("test_", 1)]
    // An index name can only contain letters, digits, and hyphens (-).
    [InlineData("test space", 1)]
    [InlineData("test/slash", 1)]
    [InlineData("test\\backslash", 1)]
    [InlineData("test.dot", 1)]
    [InlineData("test:colon", 1)]
    [InlineData("test*asterisk", 1)]
    [InlineData("test<less", 1)]
    [InlineData("test>greater", 1)]
    [InlineData("test|pipe", 1)]
    [InlineData("test?question", 1)]
    [InlineData("test\"quote", 1)]
    [InlineData("test'quote", 1)]
    [InlineData("test`backtick", 1)]
    [InlineData("test~tilde", 1)]
    [InlineData("test!exclamation", 1)]
    // Avoid names that are dot-only or dot and numbers
    [InlineData(".", 1)]
    [InlineData("..", 1)]
    [InlineData("1.2.3", 1)]
    public void BadIndexNamesAreRejected(string indexName, int errorCount)
    {
        // Creates the index using IMemoryDb
        var exception = Assert.Throws<InvalidIndexNameException>(() =>
        {
            ElasticsearchIndexname.Convert(indexName);
        });

        Assert.Equal(errorCount, exception.Errors.Count());
    }

    public void IndexNameCannotBeLongerThan255Bytes()
    {
        var indexName = new string('a', 256);
        var exception = Assert.Throws<InvalidIndexNameException>(() =>
        {
            ElasticsearchIndexname.Convert(indexName);
        });

        Assert.Equal(1, exception.Errors.Count());
    }
}
