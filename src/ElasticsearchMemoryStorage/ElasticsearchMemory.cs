// Copyright (c) Free Mind Labs, Inc. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.Mapping;
using Microsoft.Extensions.Logging;
using Microsoft.KernelMemory;
using Microsoft.KernelMemory.Diagnostics;
using Microsoft.KernelMemory.MemoryStorage;

namespace FreeMindLabs.KernelMemory.Elasticsearch;

/// <summary>
/// Elasticsearch connector for Kernel Memory.
/// </summary>
public class ElasticsearchMemory : IMemoryDb
{
    private readonly ElasticsearchConfig _config;
    private readonly ILogger<ElasticsearchMemory> _log;
    private readonly ElasticsearchClient _client;

    /// <summary>
    /// Create a new instance of Elasticsearch KM connector
    /// </summary>
    /// <param name="config">Elasticsearch configuration</param>
    /// <param name="log">Application logger</param>
    public ElasticsearchMemory(
        ElasticsearchConfig config,
        ILogger<ElasticsearchMemory>? log = null)
    {
        this._config = config ?? throw new ArgumentNullException(nameof(config));
        this._client = new ElasticsearchClient(this._config.ToElasticsearchClientSettings());
        this._log = log ?? DefaultLogger<ElasticsearchMemory>.Instance;
    }

    /// <inheritdoc />
    public async Task CreateIndexAsync(
        string index,
        int vectorSize,
        CancellationToken cancellationToken = default)
    {
        index = index ?? throw new ArgumentNullException(nameof(index));
        var existsResponse = await this._client.Indices.ExistsAsync(index, cancellationToken).ConfigureAwait(false);
        if (existsResponse.Exists)
        {
            //this._log.LogInformation($"Index {index} already exists");
            return;
        }

        var createIdxResponse = await this._client.Indices.CreateAsync(index, cancellationToken).ConfigureAwait(false);

        const int Dimensions = 1536;

        var np = new NestedProperty()
        {
            Properties = new Properties()
            {
                { ElasticsearchTag.NameField, new KeywordProperty() },
                { ElasticsearchTag.ValueField, new TextProperty() }
            }
        };

        var mapResponse = await this._client.Indices.PutMappingAsync(index, x => x
            .Properties<ElasticsearchMemoryRecord>(p =>
            {
                p.Keyword(x => x.Id);
                p.Nested(ElasticsearchMemoryRecord.TagsField, np);
                p.Text(x => x.Payload);
                p.DenseVector(x => x.Vector, d => d.Index(true).Dims(Dimensions).Similarity("cosine"));
            }),
            cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<string>> GetIndexesAsync(
        CancellationToken cancellationToken = default)
    {
        Indices indices = "";
        var resp = await this._client.Indices.GetAsync(indices, cancellationToken).ConfigureAwait(false);

        var names = resp.Indices.Select(x => x.Key.ToString());
        return names;
    }

    /// <inheritdoc />
    public async Task DeleteIndexAsync(
        string index,
        CancellationToken cancellationToken = default)
    {
        var delResponse = await this._client.Indices.DeleteAsync(index, cancellationToken).ConfigureAwait(false);
    }


    /// <inheritdoc />
    public async Task DeleteAsync(
        string index,
        MemoryRecord record,
        CancellationToken cancellationToken = default)
    {
        record = record ?? throw new ArgumentNullException(nameof(record));
        var delResponse = await this._client.DeleteAsync<ElasticsearchMemoryRecord>(
            index,
            record.Id,
            (delReq) =>
            {
            },
            cancellationToken)
            .ConfigureAwait(false);

    }

    /// <inheritdoc />
    public async Task<string> UpsertAsync(
        string index,
        MemoryRecord record,
        CancellationToken cancellationToken = default)
    {
        var response = await this._client.UpdateAsync<ElasticsearchMemoryRecord, ElasticsearchMemoryRecord>(
            index,
            record.Id,
            (updateReq) =>
            {
                var memRec = ElasticsearchMemoryRecord.FromMemoryRecord(record);
                updateReq.Doc(memRec);
                updateReq.DocAsUpsert(true);
            },
            cancellationToken)
            .ConfigureAwait(false);

        return response.Id;
    }

    /// <inheritdoc />
    public async IAsyncEnumerable<(MemoryRecord, double)> GetSimilarListAsync(
        string index,
        string text,
        ICollection<MemoryFilter>? filters = null,
        double minRelevance = 0, int limit = 1, bool withEmbeddings = false, CancellationToken cancellationToken = default)
    {
        if (filters != null)
        {
            foreach (MemoryFilter filter in filters)
            {
                if (filter is ElasticsearchMemoryFilter extendedFilter)
                {
                    // use ElasticsearchMemoryFilter filtering logic
                }

                // use MemoryFilter filtering logic
            }
        }

        var resp = await this._client.SearchAsync<ElasticsearchMemoryRecord>(s =>
            s.Index(index)
             .Query(q => q.MatchAll()),
             cancellationToken)
            .ConfigureAwait(false);

        foreach (var hit in resp.Hits)
        {
            yield return (hit.Source!.ToMemoryRecord(), hit.Score ?? 0);
        }
    }

    /// <inheritdoc />
    public IAsyncEnumerable<MemoryRecord> GetListAsync(
        string index,
        ICollection<MemoryFilter>? filters = null,
        int limit = 1,
        bool withEmbeddings = false,
        CancellationToken cancellationToken = default)
    {
        if (filters != null)
        {
            foreach (MemoryFilter filter in filters)
            {
                if (filter is ElasticsearchMemoryFilter extendedFilter)
                {
                    // use ElasticsearchMemoryFilter filtering logic
                }

                // use MemoryFilter filtering logic
            }
        }

        throw new NotImplementedException();
    }
}
