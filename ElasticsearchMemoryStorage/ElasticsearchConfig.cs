// Copyright (c) Free Mind Labs, Inc. All rights reserved.

namespace FreeMindLabs.KernelMemory.Elasticsearch;

/// <summary>
/// Elasticsearch configuration
/// </summary>
public class ElasticsearchConfig
{
    /// <summary>
    /// Connection string required to connect to Elasticsearch
    /// </summary>
    public string ConnString { get; set; } = string.Empty;
}
