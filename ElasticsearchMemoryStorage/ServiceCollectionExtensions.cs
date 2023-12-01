// Copyright (c) Free Mind Labs, Inc. All rights reserved.

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
    /// <param name="config">Elasticsearch configuration</param>
    public static IServiceCollection AddElasticsearchAsVectorDb(this IServiceCollection services, ElasticsearchConfig config)
    {
        return services
            .AddSingleton<ElasticsearchConfig>(config)
            .AddSingleton<IMemoryDb, ElasticsearchMemory>();
    }

    /// <summary>
    /// Inject Elasticsearch as the default implementation of IVectorDb
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <param name="certificateFingerPrint">Elasticsearch certificate fingerprint</param>
    /// <param name="endpoint">Elasticsearch endpoint</param>
    /// <param name="password">Elasticsearch password</param>
    /// <param name="userName">Elasticsearch username</param>
    public static IServiceCollection AddElasticsearchAsVectorDb(this IServiceCollection services,
        string endpoint, string userName, string password, string certificateFingerPrint)
    {
        var config = new ElasticsearchConfig(
            endpoint: endpoint,
            userName: userName,
            password: password,
            certificateFingerPrint: certificateFingerPrint);

        return services.AddElasticsearchAsVectorDb(config);
    }
}
