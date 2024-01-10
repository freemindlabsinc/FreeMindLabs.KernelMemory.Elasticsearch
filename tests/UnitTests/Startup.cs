// Copyright (c) Free Mind Labs, Inc. All rights reserved.

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.KernelMemory;
using System.Reflection;
using Microsoft.KernelMemory.ContentStorage.DevTools;
using FreeMindLabs.KernelMemory.Elasticsearch.Extensions;
using FreeMindLabs.KernelMemory.Elasticsearch;
using Microsoft.KernelMemory.FileSystem.DevTools;

namespace UnitTests;

/// <summary>
/// Sets up dependency injection for unit tests.
/// </summary>
public class Startup
{
    private readonly IConfiguration _configuration;

    public Startup()
    {
        // We read from the local appSettings.json and the same user secrets
        // as the Microsoft Semantic Kernel team.
        this._configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .AddUserSecrets(Assembly.GetExecutingAssembly()) // Same secrets as SK and KM :smile:
            .Build();
    }

    public void ConfigureServices(IServiceCollection services)
    {
        // We use the same OpenAI API key as in Kernel Memory.
        const string OpenAIKeyPath = "KernelMemory:Services:OpenAI:APIKey";

        // TODO: Uses only OpenAI API stuff for now. Make more flexible.
        var openApiKey = this._configuration[OpenAIKeyPath] ?? throw new ElasticsearchConfigurationException($"OpenAI API key is required. [path: {OpenAIKeyPath}]");

        // Kernel Memory with Elasticsearch
        IKernelMemoryBuilder kmBldr = new KernelMemoryBuilder(services)
                .WithSimpleFileStorage(new SimpleFileStorageConfig()
                {
                    Directory = "ContentStorage",
                    StorageType = FileSystemTypes.Volatile
                })
                .WithElasticsearch(esBldr =>
                {
                    esBldr.WithConfiguration(this._configuration);

                    // Alternatively we can use the other builder methods:                    
                    //esBldr.WithEndpoint(ElasticsearchConfigBuilder.DefaultEndpoint)
                    //      .WithIndexPrefix(ElasticsearchConfigBuilder.DefaultIndexPrefix)
                    //      .WithCertificateFingerPrint(string.Empty)
                    //      .WithUserNameAndPassword(ElasticsearchConfigBuilder.DefaultUserName, string.Empty)
                    //      .WithIndexPrefix("km.");

                })
                .WithOpenAIDefaults(apiKey: openApiKey);

        var kernelMemory = kmBldr.Build<MemoryServerless>();

        services.AddSingleton<IKernelMemory>(kernelMemory);
    }
}
