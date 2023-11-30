// Copyright (c) Free Mind Labs, Inc. All rights reserved.

using Microsoft.Extensions.DependencyInjection;
using Microsoft.KernelMemory;
using Microsoft.KernelMemory.MemoryStorage;

namespace FreeMindLabs.KernelMemory.Elasticsearch;

/// <summary>
/// Extensions for KernelMemoryBuilder
/// </summary>
public static partial class KernelMemoryBuilderExtensions
{
    /// <summary>
    /// Kernel Memory Builder extension method to add Elasticsearch memory connector.
    /// </summary>
    /// <param name="builder">KM builder instance</param>
    /// <param name="config">Elasticsearch configuration</param>
    public static IKernelMemoryBuilder WithElasticsearch(this IKernelMemoryBuilder builder, ElasticsearchConfig config)
    {
        builder.Services.AddElasticsearchAsVectorDb(config);
        return builder;
    }

    /// <summary>
    /// Kernel Memory Builder extension method to add Elasticsearch memory connector.
    /// </summary>
    /// <param name="builder">KM builder instance</param>
    /// <param name="connString">Elasticsearch connection string</param>
    public static IKernelMemoryBuilder WithElasticsearch(this IKernelMemoryBuilder builder, string connString)
    {
        builder.Services.AddElasticsearchAsVectorDb(connString);
        return builder;
    }
}

/// <summary>
/// Extensions for KernelMemoryBuilder and generic DI
/// </summary>
public static partial class DependencyInjection
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
            .AddSingleton<IVectorDb, ElasticsearchMemory>();
    }

    /// <summary>
    /// Inject Elasticsearch as the default implementation of IVectorDb
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <param name="connString">Elasticsearch connection string</param>
    public static IServiceCollection AddElasticsearchAsVectorDb(this IServiceCollection services, string connString)
    {
        var config = new ElasticsearchConfig { ConnString = connString };
        return services.AddElasticsearchAsVectorDb(config);
    }
}
