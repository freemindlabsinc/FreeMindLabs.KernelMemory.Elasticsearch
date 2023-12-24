// Copyright (c) Free Mind Labs, Inc. All rights reserved.
using Elastic.Clients.Elasticsearch;
using FreeMindLabs.KernelMemory.Elasticsearch;
using Microsoft.KernelMemory;
using Xunit;
using Xunit.Abstractions;

namespace UnitTests.Kernel;
public class KernelMemoryTests : ElasticsearchTestBase
{
    public KernelMemoryTests(ITestOutputHelper output, IKernelMemory kernelMemory, ElasticsearchClient client)
        : base(output, client)
    {
        this.KernelMemory = kernelMemory ?? throw new ArgumentNullException(nameof(kernelMemory));
    }

    public IKernelMemory KernelMemory { get; }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1305:Specify IFormatProvider", Justification = "<Pending>")]
    public async Task CanImportOneDocumentAndAskAsync()
    {
        // Cleans up the index if it exists
        var actualIndexName = FreeMindLabs.KernelMemory.Elasticsearch.ESIndexName.Convert(nameof(CanImportOneDocumentAndAskAsync));
        var delResp = await this.Client.Indices.DeleteAsync(indices: actualIndexName).ConfigureAwait(false);
        Assert.True(delResp.IsSuccess());

        // Imports a document into the index
        var id = await this.KernelMemory.ImportDocumentAsync(
            filePath: "Data/file1-Wikipedia-Carbon.txt",
            documentId: "doc001",
            tags: new TagCollection
            {
                { "indexedOn", DateTime.UtcNow.ToString("yyyy-MM-dd'T'HH:mm:ss.fffzzz") }
            },
            index: actualIndexName,
            steps: null)
            .ConfigureAwait(false);

        this.Output.WriteLine($"Indexed as '{id}'.");

        // Asks a question on the data we just inserted
        const string NoAnswer = "INFO NOT FOUND";
        var question = "What can carbon bond to?";

        MemoryAnswer? answer = null;
        // We need to wait a bit for the indexing to complete, so this is why we retry a few times.
        // TODO: add Polly.
        for (int i = 0; i < 3; i++)
        {
            answer = await this.KernelMemory.AskAsync(
                question: question,
                index: actualIndexName,
                filter: null,
                filters: null,
                minRelevance: 0)
                .ConfigureAwait(false);

            if (answer.Result != NoAnswer)
            {
                break;
            }

            await Task.Delay(500).ConfigureAwait(false);
        }

        Assert.NotNull(answer);

        this.Output.WriteLine(
            $"Q: {question}\n" +
            $"A: {answer.Result}\n");

        foreach (var cit in answer.RelevantSources)
        {
            this.Output.WriteLine($"SOURCE: {cit.DocumentId}[{cit.SourceName}] {cit.Link}\n");

            var idx = 1;
            foreach (var part in cit.Partitions)
            {
                this.Output.WriteLine($"PARTITION {idx++}. Relevance: {part.Relevance}, Last Update: {part.LastUpdate}.");
                this.Output.WriteLine($"{part.Text}");
                this.Output.WriteLine($"END OF PARTITION\n");
            }
        }
    }
}

