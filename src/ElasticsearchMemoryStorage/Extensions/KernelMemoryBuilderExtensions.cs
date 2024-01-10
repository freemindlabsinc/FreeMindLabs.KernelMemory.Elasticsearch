// Copyright (c) Free Mind Labs, Inc. All rights reserved.

using Microsoft.Extensions.Configuration;
using Microsoft.KernelMemory;

// It is easier to just extend the Microsoft.KernelMemory namespace rather than
// forcing developers to include something like FreeMindLabs.KernelMemory.Elasticsearch.

namespace FreeMindLabs.KernelMemory.Elasticsearch.Extensions;

/// <summary>
/// Extensions for KernelMemoryBuilder
/// </summary>
public static partial class KernelMemoryBuilderExtensions
{
    ///// <summary>
    ///// Kernel Memory Builder extension method to add the Elasticsearch memory connector.
    ///// </summary>
    ///// <param name="builder">The IKernelMemoryBuilder instance</param>
    ///// <param name="certificateFingerPrint">Elasticsearch certificate fingerprint</param>
    ///// <param name="endpoint">Elasticsearch endpoint</param>
    ///// <param name="password">Elasticsearch password</param>
    ///// <param name="userName">Elasticsearch username</param>
    //[Obsolete("Use the WithElasticsearch() with the builder")]
    //public static IKernelMemoryBuilder WithElasticsearch(this IKernelMemoryBuilder builder,
    //    string endpoint, string userName, string password, string certificateFingerPrint)
    //{
    //    var cfg = new ElasticsearchConfigBuilder()
    //        .WithEndpoint(endpoint)
    //        .WithUserNameAndPassword(userName, password)
    //        .WithCertificateFingerPrint(certificateFingerPrint)
    //        .Build();

    //    builder.Services.AddElasticsearchAsVectorDb(cfg);
    //    return builder;
    //}

    /// <summary>
    /// Extension method to add the Elasticsearch memory connector.
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="configure"></param>
    /// <returns></returns>
    public static IKernelMemoryBuilder WithElasticsearch(this IKernelMemoryBuilder builder, Action<ElasticsearchConfigBuilder> configure)
    {
        ArgumentNullException.ThrowIfNull(configure, nameof(configure));

        var cfg = new ElasticsearchConfigBuilder();
        configure(cfg);

        builder.Services.AddElasticsearchAsVectorDb(cfg.Build());
        return builder;
    }

    /// <summary>
    /// Kernel Memory Builder extension method to add the Elasticsearch memory connector.
    /// </summary>
    /// <param name="builder">The IKernelMemoryBuilder instance</param>
    /// <param name="configuration">The application configuration</param>"
    public static IKernelMemoryBuilder WithElasticsearch(this IKernelMemoryBuilder builder,
        IConfiguration configuration)
    {
        builder.Services.AddElasticsearchAsVectorDb(configuration);

        return builder;
    }
}
