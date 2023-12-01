// Copyright (c) Free Mind Labs, Inc. All rights reserved.

using FreeMindLabs.KernelMemory.Elasticsearch;

#pragma warning disable IDE0130 // Namespace does not match folder structure

// It is easier to just extend the Microsoft.KernelMemory namespace rather than
// forcing developers to include something like FreeMindLabs.KernelMemory.Elasticsearch.

namespace Microsoft.KernelMemory;
#pragma warning restore IDE0130 // Namespace does not match folder structure

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
    /// <param name="certificateFingerPrint">Elasticsearch certificate fingerprint</param>
    /// <param name="endpoint">Elasticsearch endpoint</param>
    /// <param name="password">Elasticsearch password</param>
    /// <param name="userName">Elasticsearch username</param>
    public static IKernelMemoryBuilder WithElasticsearch(this IKernelMemoryBuilder builder,
        string endpoint, string userName, string password, string certificateFingerPrint)
    {
        builder.Services.AddElasticsearchAsVectorDb(
            endpoint: endpoint,
            userName: userName,
            password: password,
            certificateFingerPrint: certificateFingerPrint);
        return builder;
    }
}
