// Copyright (c) Free Mind Labs, Inc. All rights reserved.

using System.ComponentModel;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;

namespace FreeMindLabs.AI.ElasticsearchMemoryPlugins;

/// <summary>
/// TBC
/// </summary>
public class ElasticsearchMemoryPlugin
{
    /// <inheritdoc />
    public ILogger<ElasticsearchMemoryPlugin> Logger { get; }

    /// <inheritdoc />
    public ElasticsearchMemoryPlugin(ILogger<ElasticsearchMemoryPlugin> logger)
    {
        this.Logger = logger;
    }

    /// <inheritdoc />
    [KernelFunction, Description("Stores information in Elasticsearch")]
    public string Store(
        [Description("The topic for the information")] string topic,
        [Description("The information")] string information)
    {
        this.Logger.LogInformation("Storing {Topic} with {Information}", topic, information);

        return $@"
<result>
    <message>OK</message>
    <topic>{topic}</topic>
    <length>{information.Length}</length>
</result>
";
    }

    /// <inheritdoc />
    [KernelFunction, Description("Retrieves information from Elasticsearch")]
    public string Retrieve(
        [Description("The topic for the information")] string topic,
        [Description("The information")] string question)
    {
        this.Logger.LogInformation("Searching for '{Question}' in {Topic}...", question, topic);

        return $@"
<result>
    <message>FOUND</message>
    <topic>{topic}</topic>
    <response>The answer is '42', just like all things...</response>
</result>
";
    }
}
