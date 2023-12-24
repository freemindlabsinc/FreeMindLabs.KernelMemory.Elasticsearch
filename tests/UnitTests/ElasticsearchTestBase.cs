// Copyright (c) Free Mind Labs, Inc. All rights reserved.
using Elastic.Clients.Elasticsearch;
using UnitTests.Memory;
using Xunit;
using Xunit.Abstractions;

namespace UnitTests;

public abstract class ElasticsearchTestBase : IAsyncLifetime
{
    protected ElasticsearchTestBase(ITestOutputHelper output, ElasticsearchClient client)
    {
        this.Output = output ?? throw new ArgumentNullException(nameof(output));
        this.Client = client ?? throw new ArgumentNullException(nameof(client));
    }

    public ITestOutputHelper Output { get; }
    public ElasticsearchClient Client { get; }

    public Task InitializeAsync()
    {
        return this.Client.DeleteIndicesOfTestAsync<DataStorageTests>();
    }

    public Task DisposeAsync()
    {
        return this.Client.DeleteIndicesOfTestAsync<DataStorageTests>();
    }
}
