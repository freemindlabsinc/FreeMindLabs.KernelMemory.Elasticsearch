// Copyright (c) Free Mind Labs, Inc. All rights reserved.
using Elastic.Clients.Elasticsearch;
using FreeMindLabs.KernelMemory.Elasticsearch;
using Microsoft.KernelMemory;
using Xunit;
using Xunit.Abstractions;

namespace UnitTests;
public class KernelMemoryTests : ElasticsearchTestBase
{
    private const string NoAnswer = "INFO NOT FOUND";

    public KernelMemoryTests(ITestOutputHelper output, IKernelMemory kernelMemory, ElasticsearchClient client, IIndexNameHelper indexNameHelper)
        : base(output, client, indexNameHelper)
    {
        this.KernelMemory = kernelMemory ?? throw new ArgumentNullException(nameof(kernelMemory));
    }

    public IKernelMemory KernelMemory { get; }

    [Fact]
    public async Task CanImportOneDocumentAndAskAsync()
    {
        var indexName = nameof(CanImportOneDocumentAndAskAsync);

        // Imports a document into the index
        var id = await this.KernelMemory.ImportDocumentAsync(
            filePath: "Data/file1-Wikipedia-Carbon.txt",
            documentId: "doc001",
            tags: new TagCollection
            {
                { "indexedOn", DateTime.UtcNow.ToString("yyyy-MM-dd'T'HH:mm:ss.fffzzz") }
            },
            index: indexName)
            .ConfigureAwait(false);

        this.Output.WriteLine($"Indexed document with id '{id}'.");

        // Waits for the documents to be saved
        var actualIndexName = this.IndexNameHelper.Convert(indexName);
        await this.Client.WaitForDocumentsAsync(actualIndexName, expectedDocuments: 2)
                  .ConfigureAwait(false);

        // Asks a question on the data we just inserted
        MemoryAnswer? answer = await this.TryToGetTopAnswerAsync(indexName, "What can carbon bond to?")
                                         .ConfigureAwait(false);
        this.PrintAnswerOfDocument(answer, "doc001");
    }

    [Fact]
    public async Task CanImportTwoDocumentsAndAskAsync()
    {
        var indexName = nameof(CanImportTwoDocumentsAndAskAsync);

        // Proceeds
        var docId = await this.KernelMemory.ImportDocumentAsync(
            "Data/file1-Wikipedia-Carbon.txt",
            index: indexName,
            documentId: "doc001").ConfigureAwait(false);

        this.Output.WriteLine($"Indexed {docId}");

        docId = await this.KernelMemory.ImportDocumentAsync(
            new Document("doc002")
                .AddFiles(new[] {
                    "Data/file2-Wikipedia-Moon.txt",
                    "Data/file3-lorem-ipsum.docx",
                    "Data/file4-SK-Readme.pdf" })
                .AddTag("user", "Blake"),
                index: indexName)
            .ConfigureAwait(false);

        this.Output.WriteLine($"Indexed {docId}");

        docId = await this.KernelMemory.ImportDocumentAsync(new Document("doc003")
            .AddFile("Data/file5-NASA-news.pdf")
            .AddTag("user", "Taylor")
            .AddTag("collection", "meetings")
            .AddTag("collection", "NASA")
            .AddTag("collection", "space")
            .AddTag("type", "news"),
            index: indexName)
            .ConfigureAwait(false);

        this.Output.WriteLine($"Indexed {docId}");

        // Waits for the documents to be saved
        var actualIndexName = this.IndexNameHelper.Convert(indexName);
        await this.Client.WaitForDocumentsAsync(actualIndexName, expectedDocuments: 10)
                         .ConfigureAwait(false);

        // This should return a citation to doc001
        var answer = await this.KernelMemory.AskAsync("What's E = m*c^2?", indexName)
                                            .ConfigureAwait(false);

        this.PrintAnswerOfDocument(answer, "doc001");

        // This should return a citation to doc002
        answer = await this.KernelMemory.AskAsync("What's Semantic Kernel?", indexName)
                                        .ConfigureAwait(false);

        this.PrintAnswerOfDocument(answer, "doc002");
    }

    private void PrintAnswerOfDocument(MemoryAnswer? answer, string expectedDocumentId)
    {
        ArgumentNullException.ThrowIfNull(answer);

        this.Output.WriteLine($"Question: {answer.Question}");
        this.Output.WriteLine($"Answer: {answer.Result}");

        var foundDocumentReference = false;
        foreach (var citation in answer.RelevantSources)
        {
            this.Output.WriteLine($"  - {citation.SourceName}  - {citation.Link} [{citation.Partitions.First().LastUpdate:D}]");

            if (citation.DocumentId == expectedDocumentId)
            {
                foundDocumentReference = true;
            }
        }

        if (!foundDocumentReference)
        {
            throw new InvalidOperationException($"It should have found a citation to document '{expectedDocumentId}'.");
        }
    }

    private async Task<MemoryAnswer?> TryToGetTopAnswerAsync(string indexName, string question)
    {
        MemoryAnswer? answer = null;

        // We need to wait a bit for the indexing to complete, so this is why we retry a few times with a delay.
        // TODO: add Polly.
        for (int i = 0; i < 3; i++)
        {
            answer = await this.KernelMemory.AskAsync(
                question: question,
                index: indexName,
                filter: null,
                filters: null,
                minRelevance: 0)
                .ConfigureAwait(false);

            if (answer.Result != NoAnswer)
            {
                break;
            }

            await Task.Delay(500)
                      .ConfigureAwait(false);
        }

        return answer;
    }
}

