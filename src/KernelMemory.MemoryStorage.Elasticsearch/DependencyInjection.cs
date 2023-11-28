using FreeMindLabs.KernelMemory.MemoryStorage.Elasticsearch;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.KernelMemory;
using Microsoft.KernelMemory.MemoryStorage;
using Microsoft.KernelMemory.MemoryStorage.Qdrant;

// ReSharper disable once CheckNamespace
namespace FreeMindLabs.KernelMemory;

public static class KernelMemoryBuilderExtensions2
{
    public static IKernelMemoryBuilder WithElasticsearch(this IKernelMemoryBuilder builder, ElasticsearchConfig config)
    {
        builder.Services.AddElasticsearchAsVectorDb(config);
        return builder;
    }

    public static IKernelMemoryBuilder WithElasticsearch(this IKernelMemoryBuilder builder, string endpoint, string apiKey = "")
    {
        builder.Services.AddElasticsearchAsVectorDb(endpoint, apiKey);
        return builder;
    }
}

public static class DependencyInjection2
{
    public static IServiceCollection AddElasticsearchAsVectorDb(this IServiceCollection services, ElasticsearchConfig config)
    {
        return services
            .AddSingleton<ElasticsearchConfig>(config)
            .AddSingleton<IVectorDb, ElasticsearchMemory>();
    }

    public static IServiceCollection AddElasticsearchAsVectorDb(this IServiceCollection services, string endpoint, string apiKey = "")
    {
        var config = new QdrantConfig { Endpoint = endpoint, APIKey = apiKey };
        return services.AddQdrantAsVectorDb(config);
    }
}
