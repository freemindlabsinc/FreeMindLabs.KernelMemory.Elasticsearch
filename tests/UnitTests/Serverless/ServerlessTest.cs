// Copyright (c) Free Mind Labs, Inc. All rights reserved.

using Microsoft.Extensions.DependencyInjection;
using Microsoft.KernelMemory;
using Xunit;
using Xunit.Abstractions;

namespace UnitTests.Serverless;

public class ServerlessTest
{
    private readonly ITestOutputHelper _output;
    private readonly IServiceProvider _services;

    public ServerlessTest(ITestOutputHelper output, IServiceProvider services)
    {
        this._output = output ?? throw new ArgumentNullException(nameof(output));
        this._services = services ?? throw new ArgumentNullException(nameof(services));
    }

    [Fact]//(Skip = "This test takes a while to complete.")]
    public async Task BehavesLikeMicrosoftMainExampleAsync()
    {
        IKernelMemory memory = this._services.GetRequiredService<IKernelMemory>();

        // Deletes the default index if already present
        await memory.DeleteIndexAsync(
        index: null,
            cancellationToken: CancellationToken.None).ConfigureAwait(false);
        this._output.WriteLine($"Ensured default index is deleted.");

        // Proceeds
        var docId = await memory.ImportDocumentAsync("file1-Wikipedia-Carbon.txt", documentId: "doc001");
        this._output.WriteLine($"Indexed {docId}");

        docId = await memory.ImportDocumentAsync(new Document("doc002")
            .AddFiles(new[] { "file2-Wikipedia-Moon.txt", "file3-lorem-ipsum.docx", "file4-SK-Readme.pdf" })
            .AddTag("user", "Blake"));
        this._output.WriteLine($"Indexed {docId}");

        docId = await memory.ImportDocumentAsync(new Document("doc003")
            .AddFile("file5-NASA-news.pdf")
            .AddTag("user", "Taylor")
            .AddTag("collection", "meetings")
            .AddTag("collection", "NASA")
            .AddTag("collection", "space")
            .AddTag("type", "news"));

        this._output.WriteLine($"Indexed {docId}");

        await Task.Delay(2000, CancellationToken.None); // TODO: remove. Without this the data might not be ready for read...

        // Question without filters
        var question = "What's E = m*c^2?";
        this._output.WriteLine($"Question: {question}");

        var answer = await memory.AskAsync(question);
        this._output.WriteLine($"\nAnswer: {answer.Result}");

        foreach (var x in answer.RelevantSources)
        {
            this._output.WriteLine($"  - {x.SourceName}  - {x.Link} [{x.Partitions.First().LastUpdate:D}]");
        }

        this._output.WriteLine("\n====================================\n");

        // Another question without filters
        question = "What's Semantic Kernel?";
        this._output.WriteLine($"Question: {question}");

        answer = await memory.AskAsync(question);
        this._output.WriteLine($"\nAnswer: {answer.Result}\n\n  Sources:\n");

        foreach (var x in answer.RelevantSources)
        {
            this._output.WriteLine($"  - {x.SourceName}  - {x.Link} [{x.Partitions.First().LastUpdate:D}]");
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

        Console.WriteLine("Test complete");
    }
}
