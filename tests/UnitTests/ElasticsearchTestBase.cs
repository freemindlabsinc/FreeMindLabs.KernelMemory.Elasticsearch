// Copyright (c) Free Mind Labs, Inc. All rights reserved.
using Elastic.Clients.Elasticsearch;
using Xunit;
using Xunit.Abstractions;

namespace UnitTests;

/// <summary>
/// A simple base class for Elasticsearch tests.
/// It ensures that all indices created by the test methods of the derived class are
/// deleted before and after the tests. This ensures that Elasticsearch is left in a clean state
/// or that subsequent tests don't fail because of left-over indices.
/// </summary>
public abstract class ElasticsearchTestBase : IAsyncLifetime
{
    protected ElasticsearchTestBase(ITestOutputHelper output, ElasticsearchClient client)
    {
        this.Output = output ?? throw new ArgumentNullException(nameof(output));
        this.Client = client ?? throw new ArgumentNullException(nameof(client));
    }

    public ITestOutputHelper Output { get; }
    public ElasticsearchClient Client { get; }
    public async Task InitializeAsync()
    {
        // Within a single test class, the tests are executed sequentially by default so
        // there is no chance for a method to finish and delete indices of other methods before the next
        // method starts executing.
        var indicesFound = await this.Client.DeleteIndicesOfTestAsync(this.GetType()).ConfigureAwait(false);

        if (indicesFound.Any())
        {
            this.Output.WriteLine($"Deleted left-over test indices: {string.Join(", ", indicesFound)}");
            this.Output.WriteLine("");
        }
    }

    public async Task DisposeAsync()
    {
        var indicesFound = await this.Client.DeleteIndicesOfTestAsync(this.GetType()).ConfigureAwait(false);

        if (indicesFound.Any())
        {
            this.Output.WriteLine($"Deleted test indices: {string.Join(", ", indicesFound)}");
            this.Output.WriteLine("");
        }
    }
}
