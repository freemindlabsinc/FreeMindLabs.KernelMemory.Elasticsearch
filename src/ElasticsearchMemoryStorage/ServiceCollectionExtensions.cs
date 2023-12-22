// Copyright (c) Free Mind Labs, Inc. All rights reserved.

using System;
using Elastic.Clients.Elasticsearch;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.KernelMemory.MemoryStorage;

namespace FreeMindLabs.KernelMemory.Elasticsearch;

/// <summary>
/// Extensions for KernelMemoryBuilder and generic DI
/// </summary>
public static partial class ServiceCollectionExtensions
{
    /// <summary>
    /// Inject Elasticsearch as the default implementation of IVectorDb
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <param name="configuration">Configuration</param>"
    public static IServiceCollection AddElasticsearchAsVectorDb(this IServiceCollection services, IConfiguration configuration)
    {
        configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));

        // Reads the configuration from IConfiguration and validates it.
        var esConfig = configuration
            .GetRequiredSection(ElasticsearchConfig.DefaultSettingsSection)
            .Get<ElasticsearchConfig>();

        return AddElasticsearchAsVectorDb(services, esConfig!);
    }

    /// <summary>
    /// Inject Elasticsearch as the default implementation of IVectorDb
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <param name="esConfig">Elasticsearch configuration</param>
    public static IServiceCollection AddElasticsearchAsVectorDb(this IServiceCollection services,
        ElasticsearchConfig esConfig)
    {
        esConfig = esConfig ?? throw new ArgumentNullException(nameof(esConfig));
        esConfig.Validate(); // This checks everything is in order.

        return services
            .AddSingleton<ElasticsearchConfig>(esConfig)
            .AddSingleton<ElasticsearchClientSettings>(x => esConfig.ToElasticsearchClientSettings())
            .AddSingleton<IMemoryDb, ElasticsearchMemory>();
    }
}
