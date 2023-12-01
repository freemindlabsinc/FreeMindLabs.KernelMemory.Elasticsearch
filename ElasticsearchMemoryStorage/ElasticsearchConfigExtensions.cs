// Copyright (c) Free Mind Labs, Inc. All rights reserved.

using System;
using Elastic.Clients.Elasticsearch;
using Elastic.Transport;

namespace FreeMindLabs.KernelMemory.Elasticsearch;

/// <summary>
/// Elasticsearch configuration extensions.
/// </summary>
public static class ElasticsearchConfigExtensions
{
    /// <summary>
    /// Validates the Elasticsearch configuration.
    /// </summary>
    /// <exception cref="ArgumentNullException">If see <paramref name="config"/> is null.</exception>
    /// <exception cref="ElasticsearchConfigurationException">If configuration values are invalid.</exception>
    public static ElasticsearchConfig Validate(this ElasticsearchConfig? config)
    {
        ArgumentNullException.ThrowIfNull(config, nameof(config));

        if (string.IsNullOrWhiteSpace(config.Endpoint))
        {
            throw new ElasticsearchConfigurationException(
                $"The {nameof(ElasticsearchConfig.Endpoint)} property is required.");
        }

        if (string.IsNullOrWhiteSpace(config.UserName))
        {
            throw new ElasticsearchConfigurationException(
                $"The {nameof(ElasticsearchConfig.UserName)} property is required.");
        }

        if (string.IsNullOrWhiteSpace(config.Password))
        {
            throw new ElasticsearchConfigurationException(
                $"The {nameof(ElasticsearchConfig.Password)} property is required.");
        }

        //TODO: validate certificate fingerprint? Add more validations?

        return config;
    }

    /// <summary>
    /// Converts an ElasticsearchConfig to a ElasticsearchClientSettings that can be used
    /// to instantiate <see cref="ElasticsearchClient"/>.
    /// </summary>
    /// <param name="config"></param>
    /// <returns></returns>
    public static ElasticsearchClientSettings ToElasticsearchClientSettings(this ElasticsearchConfig config)
    {
        ArgumentNullException.ThrowIfNull(config, nameof(config));

        // TODO: figure out the Dispose issue. It does not feel right.
        // See https://www.elastic.co/guide/en/elasticsearch/client/net-api/current/_options_on_elasticsearchclientsettings.html         
#pragma warning disable CA2000 // Dispose objects before losing scope
        return new ElasticsearchClientSettings(new Uri(config.Endpoint))

            // TODO: this needs to be more flexible.
            .Authentication(new BasicAuthentication(config.UserName, config.Password))

            // TODO: Not sure why I need this. Verify configuration maybe?
            .ServerCertificateValidationCallback((sender, certificate, chain, errors) => true)

            .CertificateFingerprint(config.CertificateFingerPrint)
            .ThrowExceptions(true) // Much easier to work with
#if DEBUG
            .DisableDirectStreaming(true);
#endif
#pragma warning restore CA2000 // Dispose objects before losing scope
    }
}
