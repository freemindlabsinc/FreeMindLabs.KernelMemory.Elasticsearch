using Elastic.Clients.Elasticsearch;
using System.Threading;
using Xunit.DependencyInjection;

namespace UnitTests;

public class ElasticsearchClientTests
{
    public class TestClass
    { 
        public long Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Length { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string[] Tags { get; set; } = Array.Empty<string>();
        public float[] NameVector { get; set; } = Array.Empty<float>();
        public float[] DescriptionVector { get; set; } = Array.Empty<float>();
    }

    [Theory]
    [InlineData("test_index01", null, default)]
    public async Task CreateIndex(string indexName, [FromServices] ElasticsearchClientSettings settings, CancellationToken cancellationToken)
    {
        var client = new ElasticsearchClient(settings ?? throw new ArgumentNullException(nameof(settings)));

        var res1 = await client.Indices.CreateAsync(
            indexName,
            s =>
            {
                s.Settings(idxSettings =>
                {
                    //idxSettings.Shards(2);
                })
                .Mappings(mapDesc =>
                {
                    var vectorSize = 384;
                    mapDesc.Properties<TestClass>(pd =>
                    {
                        pd.LongNumber("Id"); // How to generate?
                        pd.Text("Name");
                        pd.Text("Length");
                        pd.Text("Description");
                        pd.Nested("Tags", np => np.Properties(pd => pd.Keyword("Tag")));
                        pd.DenseVector("NameVector", dv => dv.Dims(vectorSize));
                        pd.DenseVector("DescriptionVector", dv => dv.Dims(vectorSize));
                    });
                });
            },
            cancellationToken);

        if (!res1.IsSuccess())
            throw new System.NotImplementedException();
    }
}