// Copyright (c) Free Mind Labs, Inc. All rights reserved.
using Elastic.Clients.Elasticsearch;
using Microsoft.KernelMemory;
using Microsoft.KernelMemory.AI;
using Microsoft.KernelMemory.MemoryStorage;
using Xunit;
using Xunit.Abstractions;

namespace UnitTests;

public class SearchTests : ElasticsearchTestBase
{
    public SearchTests(ITestOutputHelper output, IMemoryDb memoryDb, ITextEmbeddingGenerator textEmbeddingGenerator, ElasticsearchClient client)
        : base(output, client)
    {
        this.MemoryDb = memoryDb ?? throw new ArgumentNullException(nameof(memoryDb));
        this.TextEmbeddingGenerator = textEmbeddingGenerator ?? throw new ArgumentNullException(nameof(textEmbeddingGenerator));
    }

    public IMemoryDb MemoryDb { get; }
    public ITextEmbeddingGenerator TextEmbeddingGenerator { get; }

    [Fact(Skip = "Unfinished")]
    public async Task CanSearchByTagsAsync()
    {
        // We upsert the file
        var docIds = await DataStorageTests.UpsertTextFilesAsync(
            memoryDb: this.MemoryDb,
            textEmbeddingGenerator: this.TextEmbeddingGenerator,
            output: this.Output,
            indexName: nameof(CanSearchByTagsAsync),
                fileNames: new[]
                {
                    "Data/file1-Wikipedia-Carbon.txt",
                    "Data/file3-lorem-ipsum.docx",
                    "Data/file5-SK-Readme.pdf"
                })
            .ConfigureAwait(false);

        // Waits for indexing to complete
        await this.Client.WaitForDocumentsAsync(nameof(CanSearchByTagsAsync), expectedDocuments: 3)
                         .ConfigureAwait(false);

        // Gets documents that are similar to the word "carbon" .
        var foundSomething = false;

        var filter = new MemoryFilter();
        filter.Add("__file_type", "text/plain");

        //var textToMatch = "carbon";
        await foreach (var result in this.MemoryDb.GetListAsync(
            index: nameof(CanSearchByTagsAsync),
            filters: new[] { filter },
            limit: 1,
            withEmbeddings: false))
        {
            this.Output.WriteLine("Found a match for filter '{Filter}': {ResultId}.", filter, result.Id);
            foundSomething = true;
        };

        Assert.True(foundSomething, "It should have found something...");
    }
}

