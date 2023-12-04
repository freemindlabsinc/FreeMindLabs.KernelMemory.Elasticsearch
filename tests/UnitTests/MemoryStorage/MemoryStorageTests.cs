﻿// Copyright (c) Free Mind Labs, Inc. All rights reserved.
using Microsoft.KernelMemory;
using Xunit;
using Xunit.Abstractions;
using Xunit.DependencyInjection;

namespace UnitTests.MemoryStorage;
public class MemoryStorageTests
{
    private readonly ITestOutputHelper _output;
    private readonly IServiceProvider _services;

    public MemoryStorageTests(ITestOutputHelper output, IServiceProvider services)
    {
        this._output = output ?? throw new ArgumentNullException(nameof(output));
        this._services = services ?? throw new ArgumentNullException(nameof(services));
    }

    [Theory]
    [InlineData(null)]
    public async Task CreatesIndexIndexingFirstDocumentAsync([FromServices] IKernelMemory memory)
    {
        await memory.DeleteIndexAsync();
        var docId = await memory.ImportDocumentAsync("file1-Wikipedia-Carbon.txt", documentId: "doc001");
        this._output.WriteLine($"Indexed {docId}");
    }
}