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
            .AddUserSecrets(Assembly.GetExecutingAssembly())
            .Build() ?? throw new ArgumentException();
    }

    public void ConfigureServices(IServiceCollection services)
    {
        const string EsSection = "Elasticsearch";
        var esConfig = this._configuration.GetSection(EsSection).Get<ElasticsearchConfig>() ?? throw new ArgumentException(EsSection);

        using var esSettings = new ElasticsearchClientSettings(new Uri(esConfig.Endpoint!));

        var auth = new BasicAuthentication(
            username: this._configuration["Elasticsearch:UserName"]!,
            password: this._configuration["Elasticsearch:Password"]!);
        esSettings.Authentication(auth)
            // TODO: Not sure why I need this. Verify configuration maybe?
            .ServerCertificateValidationCallback((sender, certificate, chain, errors) => true)
            .CertificateFingerprint(this._configuration["Elasticsearch:CertificateFingerPrint"]!);

        esSettings.DisableDirectStreaming(true)
                  .ThrowExceptions(true);

        // ElasticsearchClientSettings
        services.AddTransient<ElasticsearchClientSettings>(x => esSettings);

        // Kernel Memory with Elasticsearch
        services.AddTransient<IKernelMemory>(x => new KernelMemoryBuilder()
            .WithOpenAIDefaults(this._configuration["OpenAI:ApiKey"]!)
            .WithElasticsearch(esConfig)
            .Build<MemoryServerless>());
    }
}
