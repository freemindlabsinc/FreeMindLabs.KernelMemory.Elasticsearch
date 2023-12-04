// Copyright (c) Free Mind Labs, Inc. All rights reserved.
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
    [InlineData(null, null)]
    public async Task CreatesIndexIndexingFirstDocumentAsync([FromServices] IKernelMemory memory, CancellationToken cancellationToken)
    {
        await memory.DeleteIndexAsync(
            index: null,
            cancellationToken: cancellationToken);

        var docId = await memory.ImportDocumentAsync(
            filePath: "file1-Wikipedia-Carbon.txt",
            documentId: "doc001",
            tags: null,
            index: null,
            steps: null,
            cancellationToken: cancellationToken
            );

        this._output.WriteLine($"Indexed {docId}");

        var question = "What is carbon?";
        var answer = await memory.AskAsync(
            question: question,
            index: null,
            filter: null,
            filters: null,
            minRelevance: 0,
            cancellationToken: cancellationToken);

        this._output.WriteLine($"Q: {question}, A: {answer}");
    }
}
