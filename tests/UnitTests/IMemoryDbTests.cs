// Copyright (c) Free Mind Labs, Inc. All rights reserved.
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
    [InlineData("", 1536, false, null)] // default index
    [InlineData("nondefault", 1536, false, null)]
    [InlineData("WithUppercase", 1536, true, null)]
    [InlineData("123numberifrst", 1536, false, null)]
    public async Task CanDeleteCreateAndDeleteIndicesAsync(string indexName, int vectorSize, bool shouldFail, [FromServices] IMemoryDb memory)
    {
        memory = memory ?? throw new ArgumentNullException(nameof(memory));

        try
        {
            await memory.DeleteIndexAsync(indexName).ConfigureAwait(false);
            this._output.WriteLine($"Attempted to delete index '{indexName}' successfully.");

            await memory.CreateIndexAsync(indexName, vectorSize).ConfigureAwait(false);

            this._output.WriteLine($"Created index '{indexName}' successfully.");

            if (shouldFail)
            {
                Assert.True(false, $"Expected exception when creating index '{indexName}'");
            }
        }
        catch (Exception ex)
        {
            if (shouldFail)
            {
                this._output.WriteLine($"Expected exception: {ex.Message}");
                return;
            }

            throw;
        }
    }
}

