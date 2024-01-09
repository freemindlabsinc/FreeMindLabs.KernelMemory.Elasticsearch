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

/// <summary>
/// The builder for ElasticsearchConfig.
/// </summary>
public class ElasticsearchConfigBuilder
{
    /// <summary>
    /// The default Elasticsearch endpoint.
    /// </summary>
    public const string DefaultEndpoint = "https://localhost:9200";

    /// <summary>
    /// The default Elasticsearch username.
    /// </summary>
    public const string DefaultUserName = "elastic";

    /// <summary>
    /// The name of the section that will contain the configuration for Elasticsearch
    /// (e.g. appSettings.json, user secrets, etc.).
    /// </summary>
    public const string DefaultSettingsSection = "Elasticsearch";

    private ElasticsearchConfig _config;

    /// <summary>
    /// The default constructor.
    /// </summary>
    public ElasticsearchConfigBuilder()
    {
        this._config = new ElasticsearchConfig();

        this.WithEndpoint(DefaultEndpoint)
            .WithCertificateFingerPrint(string.Empty)
            .WithUserNameAndPassword(DefaultUserName, string.Empty);
    }

    /// <summary>
    /// Sets Elasticsearch endpoint to connect to.
    /// </summary>
    /// <param name="endpoint"></param>
    /// <returns></returns>
    public ElasticsearchConfigBuilder WithEndpoint(string endpoint)
    {
        this._config.Endpoint = endpoint;
        return this;
    }

    /// <summary>
    /// Sets the username and password used to connect to Elasticsearch.
    /// </summary>
    /// <param name="userName"></param>
    /// <param name="password"></param>
    /// <returns></returns>
    public ElasticsearchConfigBuilder WithUserNameAndPassword(string userName, string password)
    {
        this._config.UserName = userName;
        this._config.Password = password;
        return this;
    }

    /// <summary>
    /// Sets the certificate fingerprint used to communicate with Elasticsearch.
    /// See <see href="https://www.elastic.co/guide/en/elasticsearch/reference/current/configuring-stack-security.html#_use_the_ca_fingerprint_5"/>.
    /// </summary>
    /// <param name="certificateFingerPrint"></param>
    /// <returns></returns>
    public ElasticsearchConfigBuilder WithCertificateFingerPrint(string certificateFingerPrint)
    {
        this._config.CertificateFingerPrint = certificateFingerPrint;
        return this;
    }

    public ElasticsearchConfigBuilder Validate()
    {
        return this;
    }

    public ElasticsearchConfig Build(bool skipValidation = false)
    {
        if (!skipValidation)
        {
            this.Validate();
        }

        return this._config;
    }
}
