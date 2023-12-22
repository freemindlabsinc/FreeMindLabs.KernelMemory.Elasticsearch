// Copyright (c) Free Mind Labs, Inc. All rights reserved.

using Microsoft.KernelMemory;

// It is easier to just extend the Microsoft.KernelMemory namespace rather than
// forcing developers to include something like FreeMindLabs.KernelMemory.Elasticsearch.

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
    /// <param name="certificateFingerPrint">Elasticsearch certificate fingerprint</param>
    /// <param name="endpoint">Elasticsearch endpoint</param>
    /// <param name="password">Elasticsearch password</param>
    /// <param name="userName">Elasticsearch username</param>
    public static IKernelMemoryBuilder WithElasticsearch(this IKernelMemoryBuilder builder,
        string endpoint, string userName, string password, string certificateFingerPrint)
    {
        var cfg = new ElasticsearchConfig(
            endpoint: endpoint,
            userName: userName,
            password: password,
            certificateFingerPrint: certificateFingerPrint);

        builder.Services.AddElasticsearchAsVectorDb(cfg);
        return builder;
    }
}
