using FreeMindLabs.KernelMemory.MemoryStorage.Elasticsearch;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.KernelMemory;
using System.Reflection;
using FreeMindLabs.KernelMemory;

namespace UnitTests;

public class Startup
{
    private static IKernelMemory CreateKernelMemory()
    {
        var configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .AddUserSecrets(Assembly.GetExecutingAssembly())
            .Build();

        const string OaiKey = "OpenAI:ApiKey";
        var openAiKey = configuration[OaiKey]
            ?? throw new ArgumentException(OaiKey);

        const string EsSection = "Elasticsearch";
        var esConfig = configuration.GetSection(EsSection).Get<ElasticsearchConfig>()
            ?? throw new ArgumentException(EsSection);


        var memory = new KernelMemoryBuilder()
            .WithOpenAIDefaults(openAiKey)
            .WithElasticsearch(esConfig)
            .Build<MemoryServerless>();

        return memory;
    }

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddTransient<IKernelMemory>(x => CreateKernelMemory());
    }
}
