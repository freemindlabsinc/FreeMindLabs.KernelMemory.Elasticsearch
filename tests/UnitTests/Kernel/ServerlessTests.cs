// Copyright (c) Free Mind Labs, Inc. All rights reserved.

using Elastic.Clients.Elasticsearch;
using FreeMindLabs.KernelMemory.Elasticsearch;
using Microsoft.KernelMemory;
using Xunit;
using Xunit.Abstractions;

namespace UnitTests.Kernel;

public class ServerlessTests : ElasticsearchTestBase
{
    public ServerlessTests(ITestOutputHelper output, ElasticsearchClient client, IKernelMemory kernelMemory)
        : base(output, client)
    {
        this.KernelMemory = kernelMemory;
    }

    public IKernelMemory KernelMemory { get; }


    [Fact]//(Skip = "This test takes a while to complete.")]
    public async Task BehavesLikeMicrosoftMainExampleAsync()
    {
        var indexName = nameof(BehavesLikeMicrosoftMainExampleAsync);

        // Deletes the default index if already present
        await this.KernelMemory.DeleteIndexAsync(
            index: indexName,
            cancellationToken: CancellationToken.None).ConfigureAwait(false);
        this.Output.WriteLine($"Ensured default index is deleted.");

        // Proceeds
        var docId = await this.KernelMemory.ImportDocumentAsync(
            "Data/file1-Wikipedia-Carbon.txt",
            index: indexName,
            documentId: "doc001").ConfigureAwait(false);
        this.Output.WriteLine($"Indexed {docId}");

        docId = await this.KernelMemory.ImportDocumentAsync(
            new Document("doc002")
                .AddFiles(new[] { "Data/file2-Wikipedia-Moon.txt", "Data/file3-lorem-ipsum.docx", "Data/file4-SK-Readme.pdf" })
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

        await Task.Delay(2000, CancellationToken.None).ConfigureAwait(false); // TODO: remove. Without this the data might not be ready for read...

        // Question without filters
        var question = "What's E = m*c^2?";
        this.Output.WriteLine($"Question: {question}");

        var answer = await this.KernelMemory.AskAsync(question, index: indexName).ConfigureAwait(false);
        this.Output.WriteLine($"\nAnswer: {answer.Result}");

        foreach (var x in answer.RelevantSources)
        {
            this.Output.WriteLine($"  - {x.SourceName}  - {x.Link} [{x.Partitions.First().LastUpdate:D}]");
        }

        this.Output.WriteLine("\n====================================\n");

        // Another question without filters
        question = "What's Semantic Kernel?";
        this.Output.WriteLine($"Question: {question}");

        answer = await this.KernelMemory.AskAsync(question, index: indexName).ConfigureAwait(false);
        this.Output.WriteLine($"\nAnswer: {answer.Result}\n\n  Sources:\n");

        foreach (var x in answer.RelevantSources)
        {
            this.Output.WriteLine($"  - {x.SourceName}  - {x.Link} [{x.Partitions.First().LastUpdate:D}]");
        }
    }

    [Fact]
    public void AllSequencesOfConfigurationsWork()
    {
        // This test was present in the postgressql adapter we took inspiration from.
        // I kept it just in case.

        // Concatenate our 'WithElasticsearch()' after 'WithOpenAIDefaults()' from the core nuget
        var test1 = new KernelMemoryBuilder()
            .WithOpenAIDefaults("api key")
            .WithElasticsearch(
                endpoint: "https://localhost:9200",
                userName: "elastic",
                password: "changeme",
                certificateFingerPrint: "1234567890"
            )
            .Build();

        // Concatenate our 'WithElasticsearch()' before 'WithOpenAIDefaults()' from the core nuget
        var test2 = new KernelMemoryBuilder()
            .WithElasticsearch(endpoint: "https://localhost:9200",
                userName: "elastic",
                password: "changeme",
                certificateFingerPrint: "1234567890")
            .WithOpenAIDefaults("api key")
            .Build();

        // Concatenate our 'WithElasticsearch()' before and after KM builder extension methods from the core nuget
        var test3 = new KernelMemoryBuilder()
            .WithSimpleFileStorage()
            .WithElasticsearch(endpoint: "https://localhost:9200",
                userName: "elastic",
                password: "changeme",
                certificateFingerPrint: "1234567890")
            .WithOpenAIDefaults("api key")
            .Build();

        this.Output.WriteLine("Test complete");
    }
}
