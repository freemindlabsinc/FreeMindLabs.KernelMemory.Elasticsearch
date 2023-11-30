using Elastic.Clients.Elasticsearch;
using Microsoft.KernelMemory;
using Microsoft.KernelMemory.MemoryStorage;
using Xunit.DependencyInjection;

namespace UnitTests;

public class ElasticsearchClientTests
{    
    [Theory]
    [InlineData("test_index01", null, default)]
    public async Task CreateIndex(string indexName, [FromServices] ElasticsearchClientSettings settings, CancellationToken cancellationToken)
    {
        var client = new ElasticsearchClient(settings);

        var delResponse = await client.Indices.DeleteAsync(indexName, cancellationToken);

        var createIdxResponse = await client.Indices.CreateAsync(indexName, cancellationToken);

        const int dimensions = 3;
        var mapResponse = await client.Indices.PutMappingAsync(indexName, x => x
            .Properties<MemoryRecord>(p =>
            {
                p.Keyword(x => x.Id);
                p.DenseVector(x => x.Vector, d => d.Index(true).Dims(dimensions).Similarity("cosine"));
                p.Nested(x => x.Tags);
                p.Nested(x => x.Payload);

            }),
            cancellationToken);

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
            { "UsedDimensions", dimensions }
        };

        //

        var memRec = new MemoryRecord()
        { 
            Id = "TEST-ID",
            Vector = new float[dimensions] { 1,2,3,},
            //Tags = tagCollection,            
            Payload = payload,
        };

        var createDocResponse = await client.IndexAsync(memRec, indexName, cancellationToken);
        
    }
}

