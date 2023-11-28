// See https://aka.ms/new-console-template for more information
using Microsoft.Extensions.Configuration;
using Microsoft.KernelMemory;
using System.Reflection;

var configuration = new ConfigurationBuilder()
            .AddUserSecrets(Assembly.GetExecutingAssembly())
            .Build();

var openAiKey = configuration["OpenAI:ApiKey"];

var memory = new KernelMemoryBuilder()
    .WithOpenAIDefaults(openAiKey)
//    // .FromAppSettings() => read "KernelMemory" settings from appsettings.json (if available), see https://github.com/microsoft/kernel-memory/blob/main/dotnet/Service/appsettings.json as an example
//    // .WithAzureBlobsStorage(new AzureBlobsConfig {...})                                              => use Azure Blobs
//    // .WithAzureCognitiveSearch(Env.Var("ACS_ENDPOINT"), Env.Var("ACS_API_KEY"))                      => use Azure Cognitive Search
//    // .WithQdrant("http://127.0.0.1:6333")                                                            => use Qdrant docker
//    // .WithAzureFormRecognizer(Env.Var("AZURE_COG_SVCS_ENDPOINT"), Env.Var("AZURE_COG_SVCS_API_KEY")) => use Azure Form Recognizer OCR
    .Build<MemoryServerless>();

var docId = await memory.ImportTextAsync("In physics, mass–energy equivalence is the relationship between mass and energy " +
                                         "in a system's rest frame, where the two quantities differ only by a multiplicative " +
                                         "constant and the units of measurement. The principle is described by the physicist " +
                                         "Albert Einstein's formula: E = m*c^2");

await memory.ImportDocumentAsync("Docs/file1-Wikipedia-Carbon.txt", documentId: "doc001");

await memory.ImportDocumentAsync(new Document("doc002")
            .AddFiles(new[] { "Docs/file2-Wikipedia-Moon.txt", "Docs/file3-lorem-ipsum.docx", "Docs/file4-SK-Readme.pdf" })
            .AddTag("user", "Blake"));

Console.WriteLine("Uploading a PDF with a news about NASA and Orion");
await memory.ImportDocumentAsync(new Document("doc003")
    .AddFile("Docs/file5-NASA-news.pdf")
    .AddTag("user", "Taylor")
    .AddTag("collection", "meetings")
    .AddTag("collection", "NASA")
    .AddTag("collection", "space")
    .AddTag("type", "news"));

// =======================
// === RETRIEVAL =========
// =======================
Console.WriteLine("\n====================================\n");

// Question without filters
var question = "What's E = m*c^2?";
Console.WriteLine($"Question: {question}");

var answer = await memory.AskAsync(question);
Console.WriteLine($"\nAnswer: {answer.Result}");

foreach (var x in answer.RelevantSources)
{
    Console.WriteLine($"  - {x.SourceName}  - {x.Link} [{x.Partitions.First().LastUpdate:D}]");
}

Console.WriteLine("\n====================================\n");

// Another question without filters
question = "What's Semantic Kernel?";
Console.WriteLine($"Question: {question}");

answer = await memory.AskAsync(question);
Console.WriteLine($"\nAnswer: {answer.Result}\n\n  Sources:\n");

foreach (var x in answer.RelevantSources)
{
    Console.WriteLine($"  - {x.SourceName}  - {x.Link} [{x.Partitions.First().LastUpdate:D}]");
}
