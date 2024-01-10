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

namespace UnitTests;

public class DataStorageTests : ElasticsearchTestBase
{
    public DataStorageTests(ITestOutputHelper output, IMemoryDb memoryDb, ITextEmbeddingGenerator textEmbeddingGenerator, ElasticsearchClient client,
        IIndexNameHelper indexNameHelper)
        : base(output, client, indexNameHelper)
    {
        this.MemoryDb = memoryDb ?? throw new ArgumentNullException(nameof(memoryDb));
        this.TextEmbeddingGenerator = textEmbeddingGenerator ?? throw new ArgumentNullException(nameof(textEmbeddingGenerator));
    }

    public IMemoryDb MemoryDb { get; }
    public ITextEmbeddingGenerator TextEmbeddingGenerator { get; }

    [Fact]
    public async Task CanUpsertOneTextDocumentAndDeleteAsync()
    {
        // We upsert the file
        var docIds = await DataStorageTests.UpsertTextFilesAsync(
           memoryDb: this.MemoryDb,
           textEmbeddingGenerator: this.TextEmbeddingGenerator,
           output: this.Output,
           indexName: nameof(CanUpsertOneTextDocumentAndDeleteAsync),
           fileNames: new[]
           {
               "Data/file1-Wikipedia-Carbon.txt"
           }).ConfigureAwait(false);

        // Waits for indexing to complete
        var indexName = this.IndexNameHelper.Convert(nameof(CanUpsertOneTextDocumentAndDeleteAsync));
        await this.Client.WaitForDocumentsAsync(indexName, expectedDocuments: 3)
                         .ConfigureAwait(false);

        // Deletes the document
        var deletes = docIds.Select(id => new MemoryRecord()
        {
            Id = id
        });

        foreach (var deleteRec in deletes)
        {
            await this.MemoryDb.DeleteAsync(nameof(CanUpsertOneTextDocumentAndDeleteAsync), deleteRec)
                               .ConfigureAwait(false);
        }

        // Verfiies that the documents are gone
        await this.Client.WaitForDocumentsAsync(nameof(CanUpsertOneTextDocumentAndDeleteAsync), expectedDocuments: 0)
                         .ConfigureAwait(false);
    }

    [Fact]
    public async Task CanUpsertTwoTextFilesAndGetSimilarListAsync()
    {
        await DataStorageTests.UpsertTextFilesAsync(
           memoryDb: this.MemoryDb,
           textEmbeddingGenerator: this.TextEmbeddingGenerator,
           output: this.Output,
           indexName: nameof(CanUpsertTwoTextFilesAndGetSimilarListAsync),
           fileNames: new[]
           {
               "Data/file1-Wikipedia-Carbon.txt",
               "Data/file2-Wikipedia-Moon.txt"
           }).ConfigureAwait(false);

        // Waits for the indexing to complete.
        var indexName = this.IndexNameHelper.Convert(nameof(CanUpsertTwoTextFilesAndGetSimilarListAsync));
        await this.Client.WaitForDocumentsAsync(indexName, expectedDocuments: 4)
                         .ConfigureAwait(false);


        // Gets documents that are similar to the word "carbon" .
        var foundSomething = false;

        var textToMatch = "carbon";
        await foreach (var result in this.MemoryDb.GetSimilarListAsync(
            index: nameof(CanUpsertTwoTextFilesAndGetSimilarListAsync),
            text: textToMatch,
            limit: 1))
        {
            this.Output.WriteLine($"Found a document matching '{textToMatch}': {result.Item1.Payload["file"]}.");
            return;
        };

        Assert.True(foundSomething, "It should have found something...");
    }

    public static string GuidWithoutDashes() => Guid.NewGuid().ToString().Replace("-", "", StringComparison.OrdinalIgnoreCase).ToLower(CultureInfo.CurrentCulture);

    public static async Task<IEnumerable<string>> UpsertTextFilesAsync(
        IMemoryDb memoryDb,
        ITextEmbeddingGenerator textEmbeddingGenerator,
        ITestOutputHelper output,
        string indexName,
        IEnumerable<string> fileNames)
    {
        ArgumentNullException.ThrowIfNull(memoryDb);
        ArgumentNullException.ThrowIfNull(textEmbeddingGenerator);
        ArgumentNullException.ThrowIfNull(output);
        ArgumentNullException.ThrowIfNull(indexName);
        ArgumentNullException.ThrowIfNull(fileNames);

        // IMemoryDb does not create the index automatically.
        await memoryDb.CreateIndexAsync(indexName, 1536)
                      .ConfigureAwait(false);

        var results = new List<string>();
        foreach (var fileName in fileNames)
        {
            // Reads the text from the file
            string fullText = await File.ReadAllTextAsync(fileName)
                                        .ConfigureAwait(false);

            // Splits the text into lines of up to 1000 tokens each
            var lines = TextChunker.SplitPlainTextLines(fullText,
                maxTokensPerLine: 1000,
                tokenCounter: null);

            // Splits the line into paragraphs
            var paragraphs = TextChunker.SplitPlainTextParagraphs(lines,
                maxTokensPerParagraph: 1000,
                overlapTokens: 100);

            output.WriteLine($"File '{fileName}' contains {paragraphs.Count} paragraphs.");

            // Indexes each paragraph as a separate document
            var paraIdx = 0;
            var documentId = GuidWithoutDashes() + GuidWithoutDashes();
            var fileId = GuidWithoutDashes();

            foreach (var paragraph in paragraphs)
            {
                var embedding = await textEmbeddingGenerator.GenerateEmbeddingAsync(paragraph)
                                                                 .ConfigureAwait(false);

                output.WriteLine($"Indexed paragraph {++paraIdx}/{paragraphs.Count}. {paragraph.Length} characters.");

                var filePartId = GuidWithoutDashes();

                var esId = $"d={documentId}//p={filePartId}";

                var mrec = new MemoryRecord()
                {
                    Id = esId,
                    Payload = new Dictionary<string, object>()
                    {
                        { "file", fileName },
                        { "text", paragraph },
                        { "vector_provider", textEmbeddingGenerator.GetType().Name },
                        { "vector_generator", "TODO" },
                        { "last_update", DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss") },
                        { "text_embedding_generator", textEmbeddingGenerator.GetType().Name }
                    },
                    Tags = new TagCollection()
                    {
                        { "__document_id", documentId },
                        { "__file_type", "text/plain" },
                        { "__file_id", fileId },
                        { "__file_part", filePartId }

                    },
                    Vector = embedding
                };

                var res = await memoryDb.UpsertAsync(indexName, mrec)
                                             .ConfigureAwait(false);

                results.Add(res);
            }

            output.WriteLine("");
        }

        return results;
    }
}

