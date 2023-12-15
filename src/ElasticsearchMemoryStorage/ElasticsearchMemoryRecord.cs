// Copyright (c) Free Mind Labs, Inc. All rights reserved.

using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.KernelMemory;
using Microsoft.KernelMemory.MemoryStorage;

namespace FreeMindLabs.KernelMemory.Elasticsearch;

/// <summary>
/// Elasticsearch record.
/// </summary>
public sealed class ElasticsearchMemoryRecord
{
    internal const string IdField = "id";
    internal const string VectorField = "embedding";

    /// <inheritdoc/>
    public const string TagsField = "tags";

    private const string PayloadField = "payload";

    private static readonly JsonSerializerOptions s_jsonOptions = new()
    {
        AllowTrailingCommas = true,
        MaxDepth = 10,
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Disallow,
        WriteIndented = false
    };

    /// <summary>
    /// TBC
    /// </summary>
    [JsonPropertyName(IdField)]
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// TBC
    /// </summary>
    [JsonPropertyName(TagsField)]
    public List<ElasticsearchTag> Tags { get; set; } = new();

    /// <summary>
    /// TBC
    /// </summary>
    [JsonPropertyName(PayloadField)]
    public string Payload { get; set; } = string.Empty;

    //public static MemoryDbSchema GetSchema(int vectorSize)
    //{
    //    return new MemoryDbSchema
    //    {
    //        Fields = new List<MemoryDbField>
    //        {
    //            new() { Name = IdField, Type = MemoryDbField.FieldType.Text, IsKey = true },
    //            new() { Name = VectorField, Type = MemoryDbField.FieldType.Vector, VectorSize = vectorSize },
    //            new() { Name = TagsField, Type = MemoryDbField.FieldType.ListOfStrings, IsFilterable = true },
    //            new() { Name = PayloadField, Type = MemoryDbField.FieldType.Text, IsFilterable = false },
    //        }
    //    };
    //}

    /// <summary>
    /// TBC
    /// </summary>
    [JsonPropertyName(VectorField)]
    [JsonConverter(typeof(Embedding.JsonConverter))]
    public Embedding Vector { get; set; } = new();

    /// <summary>
    /// TBC
    /// </summary>
    public MemoryRecord ToMemoryRecord(bool withEmbedding = true)
    {
        MemoryRecord result = new()
        {
            Id = DecodeId(this.Id),
            Payload = JsonSerializer.Deserialize<Dictionary<string, object>>(this.Payload, s_jsonOptions)
                      ?? new Dictionary<string, object>()
        };

        if (withEmbedding)
        {
            result.Vector = this.Vector;
        }

        foreach (var tag in this.Tags)
        {
            result.Tags.Add(tag.Name, tag.Value);
        }

        return result;
    }

    /// <summary>
    /// TBC
    /// </summary>
    /// <param name="record"></param>
    /// <returns></returns>
    public static ElasticsearchMemoryRecord FromMemoryRecord(MemoryRecord record)
    {
        ElasticsearchMemoryRecord result = new()
        {
            Id = EncodeId(record.Id),
            Vector = record.Vector,
            Payload = JsonSerializer.Serialize(record.Payload, s_jsonOptions)
        };

        foreach (var tag in record.Tags)
        {
            if ((tag.Value == null) || (tag.Value.Count == 0))
            {
                // Key only, with no values
                result.Tags.Add(new ElasticsearchTag(name: tag.Key));
                continue;
            }

            foreach (var value in tag.Value)
            {
                // Key with one or more values
                result.Tags.Add(new ElasticsearchTag(name: tag.Key, value: value));
            }
        }

        return result;
    }

    private static string EncodeId(string realId)
    {
        var bytes = Encoding.UTF8.GetBytes(realId);
        return Convert.ToBase64String(bytes).Replace('=', '_');
    }

    private static string DecodeId(string encodedId)
    {
        var bytes = Convert.FromBase64String(encodedId.Replace('_', '='));
        return Encoding.UTF8.GetString(bytes);
    }
}
