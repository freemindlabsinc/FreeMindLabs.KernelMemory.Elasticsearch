// Copyright (c) Free Mind Labs, Inc. All rights reserved.
using Microsoft.Extensions.DependencyInjection;
using Microsoft.KernelMemory;
using Microsoft.KernelMemory.MemoryStorage;
using Xunit;
using Xunit.Abstractions;
using Xunit.DependencyInjection;

namespace UnitTests;

public class IMemoryDbTests
{
    private readonly ITestOutputHelper _output;

    public IMemoryDbTests(ITestOutputHelper output, IServiceProvider sp, IKernelMemory mem)
    {
        this._output = output ?? throw new ArgumentNullException(nameof(output));

        mem = mem ?? throw new ArgumentNullException(nameof(mem));
        var memDb = sp.GetRequiredService<IMemoryDb>();
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
            await memory.CreateIndexAsync(indexName, vectorSize).ConfigureAwait(false);
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

