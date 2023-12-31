﻿// Copyright (c) Free Mind Labs, Inc. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.Mapping;
using Elastic.Clients.Elasticsearch.QueryDsl;
using Microsoft.Extensions.Logging;
using Microsoft.KernelMemory;
using Microsoft.KernelMemory.AI;
using Microsoft.KernelMemory.Diagnostics;
using Microsoft.KernelMemory.MemoryStorage;

namespace FreeMindLabs.KernelMemory.Elasticsearch;

/// <summary>
/// Elasticsearch connector for Kernel Memory.
/// </summary>
public class ElasticsearchMemory : IMemoryDb
{
    private readonly ITextEmbeddingGenerator _embeddingGenerator;
    private readonly ElasticsearchConfig _config;
    private readonly ILogger<ElasticsearchMemory> _log;
    private readonly ElasticsearchClient _client;

    /// <summary>
    /// Create a new instance of Elasticsearch KM connector
    /// </summary>
    /// <param name="config">Elasticsearch configuration</param>
    /// <param name="log">Application logger</param>
    /// <param name="embeddingGenerator">Embedding generator</param>
    public ElasticsearchMemory(
        ElasticsearchConfig config,
        ITextEmbeddingGenerator embeddingGenerator,
        ILogger<ElasticsearchMemory>? log = null)
    {
        this._embeddingGenerator = embeddingGenerator ?? throw new ArgumentNullException(nameof(embeddingGenerator));
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
        index = ESIndexName.Convert(index);

        var existsResponse = await this._client.Indices.ExistsAsync(index, cancellationToken).ConfigureAwait(false);
        if (existsResponse.Exists)
        {
            this._log.LogTrace("{MethodName}: Index {Index} already exists.", nameof(CreateIndexAsync), index);
            return;
        }

        var createIdxResponse = await this._client.Indices.CreateAsync(index, cancellationToken).ConfigureAwait(false);

        const int Dimensions = 1536; // TODO: make not hardcoded

        var np = new NestedProperty()
        {
            Properties = new Properties()
            {
                { ElasticsearchTag.NameField, new KeywordProperty() },
                { ElasticsearchTag.ValueField, new KeywordProperty() }
            }
        };

        var mapResponse = await this._client.Indices.PutMappingAsync(index, x => x
            .Properties<ElasticsearchMemoryRecord>(p =>
            {
                p.Keyword(x => x.Id);
                p.Nested(ElasticsearchMemoryRecord.TagsField, np);
                p.Text(x => x.Payload, pd => pd.Index(false));
                p.Text(x => x.Content);
                p.DenseVector(x => x.Vector, d => d.Index(true).Dims(Dimensions).Similarity("cosine"));
                // TODO: add some kind of customization routine the user can utilize when setting up DI
            }),
            cancellationToken).ConfigureAwait(false);

        this._log.LogTrace("{MethodName}: Index {Index} creeated.", nameof(CreateIndexAsync), index);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<string>> GetIndexesAsync(
        CancellationToken cancellationToken = default)
    {
        var resp = await this._client.Indices.GetAsync(Indices.All, cancellationToken).ConfigureAwait(false);

        var names = resp.Indices
            .Select(x => x.Key.ToString())
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        this._log.LogTrace("{MethodName}: Returned {IndexCount} indices: {Indices}.", nameof(GetIndexesAsync), names.Count, string.Join(", ", names));

        return names;
    }

    /// <inheritdoc />
    public async Task DeleteIndexAsync(
        string index,
        CancellationToken cancellationToken = default)
    {
        var delResponse = await this._client.Indices.DeleteAsync(
            ESIndexName.Convert(index),
            cancellationToken).ConfigureAwait(false);

        if (delResponse.IsSuccess())
        {
            this._log.LogTrace("{MethodName}: Index {Index} deleted.", nameof(DeleteIndexAsync), index);
        }
        else
        {
            this._log.LogWarning("{MethodName}: Index {Index} delete failed.", nameof(DeleteIndexAsync), index);
        }
    }

    /// <inheritdoc />
    public async Task DeleteAsync(
        string index,
        MemoryRecord record,
        CancellationToken cancellationToken = default)
    {
        record = record ?? throw new ArgumentNullException(nameof(record));

        var delResponse = await this._client.DeleteAsync<ElasticsearchMemoryRecord>(
            ESIndexName.Convert(index),
            record.Id,
            (delReq) =>
            {
            },
            cancellationToken)
            .ConfigureAwait(false);

        if (delResponse.IsSuccess())
        {
            this._log.LogTrace("{MethodName}: Record {RecordId} deleted.", nameof(DeleteAsync), record.Id);
        }
        else
        {
            this._log.LogWarning("{MethodName}: Record {RecordId} delete failed.", nameof(DeleteAsync), record.Id);
        }
    }

    /// <inheritdoc />
    public async Task<string> UpsertAsync(
        string index,
        MemoryRecord record,
        CancellationToken cancellationToken = default)
    {
        var memRec = ElasticsearchMemoryRecord.FromMemoryRecord(record);

        var response = await this._client.UpdateAsync<ElasticsearchMemoryRecord, ElasticsearchMemoryRecord>(
            ESIndexName.Convert(index),
            memRec.Id,
            (updateReq) =>
            {
                var memRec2 = memRec;
                updateReq.Doc(memRec2);
                updateReq.DocAsUpsert(true);
            },
            cancellationToken)
            .ConfigureAwait(false);

        if (response.IsSuccess())
        {
            this._log.LogTrace("{MethodName}: Record {RecordId} upserted.", nameof(UpsertAsync), memRec.Id);
        }
        else
        {
            this._log.LogError("{MethodName}: Record {RecordId} upsert failed.", nameof(UpsertAsync), memRec.Id);
        }

        return response.Id;
    }

    /// <inheritdoc />
    public async IAsyncEnumerable<(MemoryRecord, double)> GetSimilarListAsync(
        string index,
        string text,
        ICollection<MemoryFilter>? filters = null,
        double minRelevance = 0, int limit = 1, bool withEmbeddings = false, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        index = ESIndexName.Convert(index);

        this._log.LogTrace("{MethodName}: Searching for '{Text}' on index '{IndexName}' with filters {Filters}. {MinRelevance} {Limit} {WithEmbeddings}",
                           nameof(GetSimilarListAsync), text, index, filters.ToDebugString(), minRelevance, limit, withEmbeddings);

        Embedding qembed = await this._embeddingGenerator.GenerateEmbeddingAsync(text, cancellationToken).ConfigureAwait(false);
        var coll = qembed.Data.ToArray();

        var resp = await this._client.SearchAsync<ElasticsearchMemoryRecord>(s =>
            s.Index(index)
             .Knn(qd =>
             {
                 qd.k(limit)
                   .Filter(q => this.ConvertTagFilters(q, limit, filters))
                   .NumCandidates(limit + 100)
                   .Field(x => x.Vector)
                   .QueryVector(coll);
             }),
             cancellationToken)
            .ConfigureAwait(false);

        foreach (var hit in resp.Hits)
        {
            if (hit?.Source == null)
            {
                continue;
            }

            this._log.LogTrace("{MethodName} Hit: {HitScore}, {HitId}", nameof(GetSimilarListAsync), hit.Score, hit.Id);
            yield return (hit.Source!.ToMemoryRecord(), hit.Score ?? 0);
        }
    }

    /// <inheritdoc />
    public async IAsyncEnumerable<MemoryRecord> GetListAsync(
        string index,
        ICollection<MemoryFilter>? filters = null,
        int limit = 1,
        bool withEmbeddings = false,
        [EnumeratorCancellation]
        CancellationToken cancellationToken = default)
    {
        this._log.LogTrace("{MethodName}: querying index '{IndexName}' with filters {Filters}. {Limit} {WithEmbeddings}",
                nameof(GetListAsync), index, filters.ToDebugString(), limit, withEmbeddings);

        index = ESIndexName.Convert(index);

        var resp = await this._client.SearchAsync<ElasticsearchMemoryRecord>(s =>
            s.Index(index)
             .Size(limit)
             .Query(qd =>
             {
                 this.ConvertTagFilters(qd, limit, filters);
             }),
             cancellationToken)
            .ConfigureAwait(false);

        foreach (var hit in resp.Hits)
        {
            if (hit?.Source == null)
            {
                continue;
            }

            this._log.LogTrace("{MethodName} Hit: {HitScore}, {HitId}", nameof(GetListAsync), hit.Score, hit.Id);
            yield return hit.Source!.ToMemoryRecord();
        }
    }

    private QueryDescriptor<ElasticsearchMemoryRecord> ConvertTagFilters(
        QueryDescriptor<ElasticsearchMemoryRecord> qd,
        int limit,
        ICollection<MemoryFilter>? filters = null)
    {
        if ((filters == null) || (filters.Count == 0))
        {
            qd.MatchAll();
            return qd;
        }

        qd.Nested(nqd =>
        {
            nqd.Path(ElasticsearchMemoryRecord.TagsField);
            nqd.Query(nq =>
            {
                // Each filter is a tag collection.
                foreach (MemoryFilter filter in filters)
                {
                    // Each tag collection is an element of a List<string, List<string?>>>
                    foreach (var tagName in filter.Keys)
                    {
                        nq.Bool(bq =>
                        {
                            List<string?> tagValues = filter[tagName];
                            List<FieldValue> terms = tagValues.Select(x => (FieldValue)(x ?? FieldValue.Null))
                                                              .ToList();

                            bq.Must(
                                t => t.Term(c => c.Field(ElasticsearchMemoryRecord.Tags_Name).Value(tagName)),
                                t => t.Terms(c => c.Field(ElasticsearchMemoryRecord.Tags_Value).Terms(new TermsQueryField(terms)))
                            );
                        });
                    }
                }
            });
        });

        return qd;
    }
}
