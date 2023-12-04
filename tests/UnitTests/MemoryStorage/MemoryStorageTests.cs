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
    public async Task CreateIndexAndSearchInDefaultIndexAsync(
        [FromServices] IKernelMemory memory,
        CancellationToken cancellationToken)
    {
        cancellationToken = CancellationToken.None;

        // Deletes the default index if already present
        await memory.DeleteIndexAsync(
            index: null,
            cancellationToken: cancellationToken).ConfigureAwait(false);
        this._output.WriteLine($"Ensured default index is deleted.");

        // Imports the document into the default index
        var docId = await memory.ImportDocumentAsync(
            filePath: "file1-Wikipedia-Carbon.txt",
            documentId: "doc001",
            tags: null,
            index: null,
            steps: null,
            cancellationToken: cancellationToken).ConfigureAwait(false);
        this._output.WriteLine($"Indexed {docId}.");

        await Task.Delay(2000, CancellationToken.None); // TODO: remove. Without this the data might not be ready for read...

        // Asks a question on the data we just inserted
        var question = "What can carbon bond to?";
        var answer = await memory.AskAsync(
            question: question,
            index: null,
            filter: null,
            filters: null,
            minRelevance: 0,
            cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        this._output.WriteLine($"Q: {question}, A: {answer.Result}");
        Assert.NotEqual("INFO NOT FOUND", answer.Result);
    }

    [Theory]
    [InlineData(null, null)]
    public async Task IndexesAllDocumentsAsync(
        [FromServices] IKernelMemory memory,
        CancellationToken cancellationToken)
    {
        cancellationToken = CancellationToken.None;
        // Deletes the default index if already present
        await memory.DeleteIndexAsync(
            index: null,
            cancellationToken: cancellationToken).ConfigureAwait(false);
        this._output.WriteLine($"Ensured default index is deleted.");

        // Uploads the documents
        var files = Directory.GetFiles(".", "*.txt").ToList();
        files.AddRange(Directory.GetFiles(".", "*.docx"));
        files.AddRange(Directory.GetFiles(".", "*.pdf"));

        foreach (var file in files)
        {
            var docId = await memory.ImportDocumentAsync(
                filePath: file,
                documentId: null,
                tags: null,
                index: null,
                steps: null,
                cancellationToken: cancellationToken).ConfigureAwait(false);

            this._output.WriteLine($"Indexed {docId}[{file}].");
        }

        //await memory.ImportDocumentAsync()
        // Imports the document into the default index
        //var docId = await memory.ImportDocumentAsync(
        //    filePath: "file1-Wikipedia-Carbon.txt",
        //    documentId: "doc001",
        //    tags: null,
        //    index: null,
        //    steps: null,
        //    cancellationToken: cancellationToken).ConfigureAwait(false);
        //this._output.WriteLine($"Indexed {docId}.");
        //
        //await Task.Delay(2000, CancellationToken.None); // TODO: remove. Without this the data might not be ready for read...
        //
        //// Asks a question on the data we just inserted
        //var question = "What can carbon bond to?";
        //var answer = await memory.AskAsync(
        //    question: question,
        //    index: null,
        //    filter: null,
        //    filters: null,
        //    minRelevance: 0,
        //    cancellationToken: cancellationToken)
        //    .ConfigureAwait(false);
        //
        //this._output.WriteLine($"Q: {question}, A: {answer.Result}");
        //Assert.NotEqual("INFO NOT FOUND", answer.Result);
    }
}
