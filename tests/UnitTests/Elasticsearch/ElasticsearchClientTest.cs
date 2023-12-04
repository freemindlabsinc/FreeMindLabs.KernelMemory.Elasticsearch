// Copyright (c) Free Mind Labs, Inc. All rights reserved.

using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.Mapping;
using FreeMindLabs.KernelMemory.Elasticsearch;
using Microsoft.KernelMemory;
using Microsoft.KernelMemory.MemoryStorage;
using Xunit;
using Xunit.DependencyInjection;

namespace UnitTests.Elasticsearch;

public class ElasticsearchClientTest
{
    [Theory]
    [InlineData("test_index01", null, default)]
    public async Task CreatesIndexCrappyAsync(string indexName, [FromServices] ElasticsearchClientSettings settings, CancellationToken cancellationToken)
    {
        // TODO: move this in the IVectorDb implementation
        var client = new ElasticsearchClient(settings);

        var delResponse = await client.Indices.DeleteAsync(indexName, cancellationToken);

        var createIdxResponse = await client.Indices.CreateAsync(indexName, cancellationToken);

        const int Dimensions = 1536;

        var np = new NestedProperty()
        {
            Properties = new Properties()
            {
                { ElasticsearchTag.NameField, new KeywordProperty() },
                { ElasticsearchTag.ValueField, new TextProperty() }
            }
        };

        var mapResponse = await client.Indices.PutMappingAsync(indexName, x => x
            .Properties<ElasticsearchMemoryRecord>(p =>
            {
                p.Keyword(x => x.Id);
                p.DenseVector(x => x.Vector, d => d.Index(true).Dims(Dimensions).Similarity("cosine"));
                p.Nested(ElasticsearchMemoryRecord.TagsField, np);
                p.Text(x => x.Payload);

            }),
            cancellationToken);

        // Key value pairs
        var kvPairs = new KeyValuePair<string, List<string?>>(
            "multipleValuesForKey",
            new List<string?>() { "value1", "value2" }
            );

        var tagCollection = new TagCollection();
        tagCollection.Add("keyOnly");
        tagCollection.Add(kvPairs);
        tagCollection.Add("KeyAndValue", "K&V value");
        tagCollection.Add("listKey", new List<string?>() { "listValue1", "listValue2" });

        var payload = new Dictionary<string, object>()
        {
            { "Id", "TEST-ID" },
            { "UsedDimensions", Dimensions }
        };

        //
        var vector = new float[Dimensions];
        Array.Fill(vector, 1.0f);

        var memRec = new MemoryRecord()
        {
            Id = "TEST-ID",
            Vector = vector,
            Tags = tagCollection,
            Payload = payload,
        };

        var esMemRec = ElasticsearchMemoryRecord.FromMemoryRecord(memRec);

        var createDocResponse = await client.IndexAsync(esMemRec, indexName, cancellationToken);

        GetResponse<ElasticsearchMemoryRecord> getResponse = await client.GetAsync<ElasticsearchMemoryRecord>(indexName, createDocResponse.Id, cancellationToken);

        var newMemRecord = getResponse.Source?.ToMemoryRecord() ?? throw new FileNotFoundException("Failed to get the document");

        Assert.Equal(memRec.Id, newMemRecord.Id);
        Assert.Equal(memRec.Tags.Count, newMemRecord.Tags.Count);
    }
}

