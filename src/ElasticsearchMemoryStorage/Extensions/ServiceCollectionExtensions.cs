// Copyright (c) Free Mind Labs, Inc. All rights reserved.

using Elastic.Clients.Elasticsearch;
using FreeMindLabs.KernelMemory.Elasticsearch.Utils;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.KernelMemory.MemoryStorage;

namespace FreeMindLabs.KernelMemory.Elasticsearch.Extensions;

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
            .GetRequiredSection(ElasticsearchConfigBuilder.DefaultSettingsSection)
            .Get<ElasticsearchConfig>();

        return services.AddElasticsearchAsVectorDb(esConfig!);
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

        // The ElasticsearchClient type is thread-safe and can be shared and
        // reused across multiple threads in consuming applications. 
        // See https://www.elastic.co/guide/en/elasticsearch/client/net-api/current/recommendations.html
        services.AddSingleton(sp =>
        {
            var esConfig = sp.GetRequiredService<ElasticsearchConfig>();
            return new ElasticsearchClient(esConfig.ToElasticsearchClientSettings());
        });

        return services
            .AddSingleton<IIndexNameHelper, IndexNameHelper>()
            .AddSingleton(esConfig)
            .AddSingleton<IMemoryDb, ElasticsearchMemory>();
    }
}
