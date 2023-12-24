// Copyright (c) Free Mind Labs, Inc. All rights reserved.
using System.Globalization;
using Elastic.Clients.Elasticsearch;
using FreeMindLabs.KernelMemory.Elasticsearch;
using Microsoft.KernelMemory;
using Microsoft.KernelMemory.AI;
using Microsoft.KernelMemory.DataFormats.Text;
using Microsoft.KernelMemory.MemoryStorage;
using Xunit;
using Xunit.Abstractions;

namespace UnitTests.Memory;

public class DataStorageTests : ElasticsearchTestBase
{
    public DataStorageTests(ITestOutputHelper output, IMemoryDb memoryDb, ITextEmbeddingGenerator textEmbeddingGenerator, ElasticsearchClient client)
        : base(output, client)
    {
        this.MemoryDb = memoryDb ?? throw new ArgumentNullException(nameof(memoryDb));
        this.TextEmbeddingGenerator = textEmbeddingGenerator ?? throw new ArgumentNullException(nameof(textEmbeddingGenerator));
    }

    public IMemoryDb MemoryDb { get; }
    public ITextEmbeddingGenerator TextEmbeddingGenerator { get; }

    [Fact]
    public async Task CanUpsertOneTextDocumentAsync()
    {
        // We upset the file
        await this.UpsertTextFilesAsync(
           indexName: nameof(CanUpsertOneTextDocumentAsync),
           fileNames: new[]
           {
               "Data/file1-Wikipedia-Carbon.txt"
           }).ConfigureAwait(false);

        // We try to read it a few times. ES takes a bit to store...
        // TODO: use Polly. This is a temporary hack.
        for (int i = 0; i < 3; i++)
        {
            var results = await this.Client
                .SearchAsync<ElasticsearchMemoryRecord>(sr =>
                {
                    sr.Index(ESIndexName.Convert(nameof(CanUpsertOneTextDocumentAsync)))
                      .Query(q => q.MatchAll());
                })
                .ConfigureAwait(false);

            if (results.Hits.Count > 0)
            {
                return;
            }
            await Task.Delay(1000).ConfigureAwait(false);
        }

        throw new InvalidOperationException("It should have found something...");
    }

    [Fact]//(Skip = "For some reason this blows up when I run it, but not when I debug it..")]
    public async Task CanUpsertTwoTextFilesAndGetSimilarListAsync()
    {
        await this.UpsertTextFilesAsync(
           indexName: nameof(CanUpsertTwoTextFilesAndGetSimilarListAsync),
           fileNames: new[]
           {
               "Data/file1-Wikipedia-Carbon.txt",
               "Data/file2-Wikipedia-Moon.txt"
           }).ConfigureAwait(false);

        // TODO: waits for the indexing to complete. This is a temporary hack.
        await Task.Delay(2000)
                  .ConfigureAwait(false);

        var foundSomething = false;
        await foreach (var para in this.MemoryDb.GetSimilarListAsync(
            index: nameof(CanUpsertTwoTextFilesAndGetSimilarListAsync),
            text: "carbon",
            limit: 2))
        {
            foundSomething = true;
            if (para.Item1 == null)
            {
                throw new InvalidOperationException("It should not be null.");
            }
            if (para.Item2 <=0)
            {
                throw new InvalidOperationException("It should be greater than zero.");
            }
            this.Output.WriteLine($"\nFound paragraph: ({para.Item2}) {para.Item1.Id}");
        };

        Assert.True(foundSomething, "It should have found something...");
    }

    private string GuidWithoutDashes() => Guid.NewGuid().ToString().Replace("-", "", StringComparison.OrdinalIgnoreCase).ToLower(CultureInfo.CurrentCulture);

    private async Task UpsertTextFileAsync(string indexName, string fileName)
    {
        string fullText = await File.ReadAllTextAsync(fileName)
                                    .ConfigureAwait(false);

        //var tokenCounter = new TextChunker.TokenCounter();

        var lines = TextChunker.SplitPlainTextLines(fullText,
            maxTokensPerLine: 1000,
            tokenCounter: null);

        var paragraphs = TextChunker.SplitPlainTextParagraphs(lines,
            maxTokensPerParagraph: 1000,
            overlapTokens: 100);

        this.Output.WriteLine($"File '{fileName}' contains {paragraphs.Count} paragraphs.");

        var paraIdx = 0;
        var fileId = this.GuidWithoutDashes();
        foreach (var paragraph in paragraphs)
        {
            var embedding = await this.TextEmbeddingGenerator.GenerateEmbeddingAsync(paragraph)
                                                             .ConfigureAwait(false);

            this.Output.WriteLine($"Indexed paragraph {++paraIdx}/{paragraphs.Count}. {paragraph.Length} characters.");

            var filePartId = this.GuidWithoutDashes();
            var id = $"d={fileId}//p={filePartId}";

            var mrec = new MemoryRecord()
            {
                Id = id,
                Payload = new Dictionary<string, object>()
                    {
                        { "file", fileName },
                        { "text", paragraph },
                        { "vector_provider", "AI.OpenAI.OpenAITextEmbeddingGenerator" },
                        { "vector_generator", "TODO" },
                        { "last_update", "2023-12-23T00:38:53" },
                        { "text_embedding_generator", this.TextEmbeddingGenerator.GetType().Name }
                    },
                Tags = new TagCollection()
                    {
                        { "__document_id", "b07f566008af4489afcd4969176d6f5f202312220738529732909" },
                        { "__file_type", "text/plain" },
                        { "__file_id", "1d3c7c2f8d8e4d428cbf54c6bbd8d9aa" },
                        { "__file_part", "bae2060ea2d145e491341c240f2d97c4" },
                    },
                Vector = embedding
            };

            var res = await this.MemoryDb.UpsertAsync(indexName, mrec)
                                         .ConfigureAwait(false);
        }
    }

    private async Task UpsertTextFilesAsync(string indexName, IEnumerable<string> fileNames)
    {
        await this.MemoryDb.DeleteIndexAsync(index: indexName)
                     .ConfigureAwait(false);

        await this.MemoryDb.CreateIndexAsync(index: nameof(CanUpsertTwoTextFilesAndGetSimilarListAsync), 1536)
                           .ConfigureAwait(false);

        foreach (var fileName in fileNames)
        {
            await this.UpsertTextFileAsync(indexName: indexName, fileName: fileName)
                      .ConfigureAwait(false);
        }
    }
}
