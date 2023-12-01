﻿// Copyright (c) Free Mind Labs, Inc. All rights reserved.

using Microsoft.Extensions.DependencyInjection;
using Microsoft.KernelMemory;
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

    [Fact]
    public async Task BehavesLikeMicrosoftMainExampleAsync()
    {
        IKernelMemory memory = this._services.GetRequiredService<IKernelMemory>();

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
}