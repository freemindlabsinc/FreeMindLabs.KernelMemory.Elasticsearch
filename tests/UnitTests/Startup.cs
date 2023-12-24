// Copyright (c) Free Mind Labs, Inc. All rights reserved.

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.KernelMemory;
using System.Reflection;
using FreeMindLabs.KernelMemory.Elasticsearch;
using Microsoft.KernelMemory.ContentStorage.DevTools;

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
        // TODO: Uses only OpenAI API stuff for now. Make more flexible.        

        // Kernel Memory with Elasticsearch
        IKernelMemoryBuilder b = new KernelMemoryBuilder(services)
                .WithSimpleFileStorage(new SimpleFileStorageConfig()
                {
                    Directory = "ContentStorage",
                    StorageType = Microsoft.KernelMemory.FileSystem.DevTools.FileSystemTypes.Volatile
                })
                .WithElasticsearch(this._configuration)
                .WithOpenAIDefaults(apiKey: this._configuration["OpenAI:ApiKey"] ?? throw new ArgumentException("OpenAI:ApiKey is required."));

        var kernelMemory = b.Build<MemoryServerless>();

        services.AddSingleton<IKernelMemory>(kernelMemory);
    }
}
