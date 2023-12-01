// Copyright (c) Free Mind Labs, Inc. All rights reserved.

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.KernelMemory;
using System.Reflection;
using Elastic.Clients.Elasticsearch;
using Elastic.Transport;
using FreeMindLabs.KernelMemory.Elasticsearch;

namespace UnitTests;

public class Startup
{
    private readonly IConfiguration _configuration;

    public Startup()
    {
        this._configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .AddUserSecrets(Assembly.GetExecutingAssembly()) // Same secrets as SK and KM :smile:
            .Build() ?? throw new ArgumentException();
    }

    public void ConfigureServices(IServiceCollection services)
    {
        // ElasticsearchClientSettings
        var esConfig = this._configuration
            .GetSection(ElasticsearchConfig.DefaultSettingsSection)
            .Get<ElasticsearchConfig>()
            .Validate(); // This checks everything is in order.

        services.AddTransient<ElasticsearchClientSettings>(x => esConfig.ToElasticsearchClientSettings());

        // Kernel Memory with Elasticsearch
        services.AddTransient<IKernelMemory>(x => new KernelMemoryBuilder()
            .WithOpenAIDefaults(this._configuration["OpenAI:ApiKey"] ?? throw new ElasticsearchConfigurationException("OpenAI API Key missing."))
            .WithElasticsearch(esConfig)
            .Build<MemoryServerless>());

        // TODO: Uses OpenAI API Key for now. Make more flexible.
    }
}
