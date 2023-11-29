// Copyright (c) Free Mind Labs, Inc. All rights reserved.

using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.IndexManagement;
using Elastic.Clients.Elasticsearch.Mapping;
using Microsoft.Extensions.Azure;
using Microsoft.KernelMemory;
using Microsoft.KernelMemory.MemoryStorage;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace FreeMindLabs.KernelMemory.MemoryStorage.Elasticsearch;

/// <summary>
/// xxx
/// </summary>
public class ElasticsearchMemory : IVectorDb
{
    public ElasticsearchMemory(ElasticsearchClientSettings settings)
    {
        Client = new ElasticsearchClient(settings);
    }

    ElasticsearchClient Client { get; }

    /// <inheritdoc/>
    public async Task CreateIndexAsync(string index, int vectorSize, CancellationToken cancellationToken = default)
    {
        var desc = new CreateIndexRequest(index)
        {
            Settings = new IndexSettings
            {
                NumberOfShards = 1,
                NumberOfReplicas = 1
            },
            //Mappings = new TypeMapping
            //{
            //    Properties = new Dictionary<string, IProperty>
            //    {
            //        { "vector", new BinaryProperty { Type = "binary", DocValues = true } },
            //        { "id", new KeywordProperty { Type = "keyword" } },
            //        { "tags", new KeywordProperty { Type = "keyword" } },
            //        { "created", new DateProperty { Type = "date" } },
            //        { "updated", new DateProperty { Type = "date" } },
            //        { "data", new ObjectProperty { Type = "object" } },
            //    }
            //}
        };
        var res1 = await Client.Indices.CreateAsync(
            index, 
            s => {
                s.Settings(idxSettings =>
                {
                    idxSettings.Shards(2);
                })
                .Mappings(mapDesc =>
                {
                    vectorSize = 384;
                    //mapDesc.Properties(pd =>
                    //{
                    //    pd.LongNumber("Id"); // How to generate?
                    //    pd.Text("Name");
                    //    pd.Text("Length");
                    //    pd.Text("Description");
                    //    pd.Nested(pd => pd.Name("Tags").Properties(pd => pd.Keyword("Tag")));
                    //    pd.DenseVector(dv => dv.Name("NameVector").Dims(vectorSize));
                    //    pd.DenseVector(dv => dv.Name("DescriptionVector").Dims(vectorSize));
                    //});
                });
                }, 
        cancellationToken: cancellationToken)
           .ConfigureAwait(false); // TODO: verify this is correct

        if (!res1.IsSuccess())
            throw new System.NotImplementedException();
    }

    /// <inheritdoc/>
    public Task DeleteAsync(string index, MemoryRecord record, CancellationToken cancellationToken = default)
    {
        throw new System.NotImplementedException();
    }

    /// <inheritdoc/>
    public Task DeleteIndexAsync(string index, CancellationToken cancellationToken = default)
    {
        throw new System.NotImplementedException();
    }

    /// <inheritdoc/>
    public Task<IEnumerable<string>> GetIndexesAsync(CancellationToken cancellationToken = default)
    {
        throw new System.NotImplementedException();
    }

    /// <inheritdoc/>
    public IAsyncEnumerable<MemoryRecord> GetListAsync(string index, ICollection<MemoryFilter>? filters = null, int limit = 1, bool withEmbeddings = false, CancellationToken cancellationToken = default)
    {
        throw new System.NotImplementedException();
    }

    /// <inheritdoc/>
    public IAsyncEnumerable<(MemoryRecord, double)> GetSimilarListAsync(string index, Embedding embedding, ICollection<MemoryFilter>? filters = null, double minRelevance = 0, int limit = 1, bool withEmbeddings = false, CancellationToken cancellationToken = default)
    {
        throw new System.NotImplementedException();
    }

    /// <inheritdoc/>
    public Task<string> UpsertAsync(string index, MemoryRecord record, CancellationToken cancellationToken = default)
    {
        throw new System.NotImplementedException();
    }
}
