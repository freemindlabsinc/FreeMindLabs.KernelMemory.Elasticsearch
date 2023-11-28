using Microsoft.Extensions.DependencyInjection;
using Microsoft.KernelMemory;
using Xunit.Abstractions;

namespace UnitTests.Serverless;

public class ServerlessTests
{
    private readonly ITestOutputHelper Output;
    private readonly IServiceProvider Services;

    public ServerlessTests(ITestOutputHelper output, IServiceProvider services)
    {
        Output = output ?? throw new ArgumentNullException(nameof(output));
        Services = services ?? throw new ArgumentNullException(nameof(services));
    }

    [Fact]
    public async Task KernelMemoryInspired()
    {
        var memory = Services.GetRequiredService<IKernelMemory>();

        var docId = await memory.ImportDocumentAsync("Docs/file1-Wikipedia-Carbon.txt", documentId: "doc001");
        Output.WriteLine($"Indexed {docId}");

        docId = await memory.ImportDocumentAsync(new Document("doc002")
            .AddFiles(new[] { "Docs/file2-Wikipedia-Moon.txt", "Docs/file3-lorem-ipsum.docx", "Docs/file4-SK-Readme.pdf" })
            .AddTag("user", "Blake"));
        Output.WriteLine($"Indexed {docId}");

        docId = await memory.ImportDocumentAsync(new Document("doc003")
            .AddFile("Docs/file5-NASA-news.pdf")
            .AddTag("user", "Taylor")
            .AddTag("collection", "meetings")
            .AddTag("collection", "NASA")
            .AddTag("collection", "space")
            .AddTag("type", "news"));

        Output.WriteLine($"Indexed {docId}");

        // Question without filters
        var question = "What's E = m*c^2?";
        Output.WriteLine($"Question: {question}");

        var answer = await memory.AskAsync(question);
        Output.WriteLine($"\nAnswer: {answer.Result}");

        foreach (var x in answer.RelevantSources)
        {
            Output.WriteLine($"  - {x.SourceName}  - {x.Link} [{x.Partitions.First().LastUpdate:D}]");
        }

        Output.WriteLine("\n====================================\n");

        // Another question without filters
        question = "What's Semantic Kernel?";
        Output.WriteLine($"Question: {question}");

        answer = await memory.AskAsync(question);
        Output.WriteLine($"\nAnswer: {answer.Result}\n\n  Sources:\n");

        foreach (var x in answer.RelevantSources)
        {
            Output.WriteLine($"  - {x.SourceName}  - {x.Link} [{x.Partitions.First().LastUpdate:D}]");
        }
    }
}