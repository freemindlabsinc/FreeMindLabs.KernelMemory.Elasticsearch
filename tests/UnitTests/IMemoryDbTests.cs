// Copyright (c) Free Mind Labs, Inc. All rights reserved.
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
    public async Task CanDeleteCreateAndDeleteIndicesAsync(string indexName, int vectorSize, [FromServices] IMemoryDb memory)
    {
        memory = memory ?? throw new ArgumentNullException(nameof(memory));

        // Delete the index if it exists
        var currIndices = await memory.GetIndexesAsync()
                                      .ConfigureAwait(false);

        if (string.IsNullOrEmpty(indexName))
        {
            indexName = "default";
        }

        if (currIndices.Contains(indexName))
        {
            await memory.DeleteIndexAsync(indexName)
                        .ConfigureAwait(false);

            this._output.WriteLine($"Index '{indexName}' already exists and has been deleted.");
        }

        // Create the index using the given name ()
        await memory.CreateIndexAsync(indexName, vectorSize)
                    .ConfigureAwait(false);

        // Verifies the index exists

        // TODO: verify the index is created using the ES client

        this._output.WriteLine($"Created index '{indexName}' successfully.");

        // Delete the index again to leave a clean slate
        await memory.DeleteIndexAsync(indexName)
                    .ConfigureAwait(false);

        this._output.WriteLine($"Index '{indexName}' has been deleted.");

        // TODO: verify the index is deleted using the ES client
    }
}

