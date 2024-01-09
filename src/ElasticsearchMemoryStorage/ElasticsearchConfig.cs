// Copyright (c) Free Mind Labs, Inc. All rights reserved.

using System;
using Elastic.Clients.Elasticsearch.Mapping;

namespace FreeMindLabs.KernelMemory.Elasticsearch;

/// <summary>
/// The configuration for Elasticsearch.
/// </summary>
public class ElasticsearchConfig
{
    /// <summary>
    /// Default constructor.
    /// </summary>
    internal ElasticsearchConfig()
    { }

    /// <summary>
    /// The certificate fingerprint for the Elasticsearch instance.
    /// See <see href="https://www.elastic.co/guide/en/elasticsearch/reference/current/configuring-stack-security.html#_use_the_ca_fingerprint_5"/>.
    /// </summary>    
    public string CertificateFingerPrint { get; internal set; } = string.Empty;

    /// <summary>
    /// The Elasticsearch endpoint.
    /// </summary>
    public string Endpoint { get; internal set; } = string.Empty;

    /// <summary>
    /// The username used to connect to Elasticsearch.
    /// </summary>
    public string UserName { get; internal set; } = string.Empty;

    /// <summary>
    /// The password used to connect to Elasticsearch.
    /// </summary>
    public string Password { get; internal set; } = string.Empty;

    /// <summary>
    /// A delegate to configure the Elasticsearch index properties.
    /// </summary>
    public Action<PropertiesDescriptor<ElasticsearchMemoryRecord>>? ConfigureProperties { get; internal set; }
}
