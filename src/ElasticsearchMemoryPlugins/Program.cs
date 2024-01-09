// Copyright (c) Free Mind Labs, Inc. All rights reserved.

using FreeMindLabs.AI.ElasticsearchMemoryPlugins;
using FreeMindLabs.AI.ElasticsearchMemoryPlugins.FromSk;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Extensions;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle

builder.Services.AddTransient<ElasticsearchMemoryPlugin>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCors();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors(builder => builder
 .AllowAnyOrigin()
 .AllowAnyMethod()
 .AllowAnyHeader()
);

app.MapPost("/store", (
    string topic,
    string information, ElasticsearchMemoryPlugin esmem) =>
{
    return esmem.Store(topic, information);
})
.WithOpenApi(genOp =>
{
    genOp.OperationId = "store";
    genOp.AddExtension("x-openai-isConsequential", new OpenApiBoolean(true));
    genOp.Description = "Stores the information in Elasticsearch, in the topic index";
    genOp.Parameters.Clear();
    genOp.Parameters.Add(new OpenApiParameter
    {
        Name = "topic",
        In = ParameterLocation.Query,
        Required = true,
        Description = "The topic"
    });
    genOp.Parameters.Add(new OpenApiParameter
    {
        Name = "information",
        In = ParameterLocation.Query,
        Required = true,
        Description = "The information as text"
    });

    return genOp;
});

app.MapGet("/retrieve", (string topic, string question, ElasticsearchMemoryPlugin esmem) =>
{
    return esmem.Retrieve(topic, question);
})
.WithOpenApi(genOp =>
{
    genOp.OperationId = "retrieve";
    genOp.AddExtension("x-openai-isConsequential", new OpenApiBoolean(true));
    genOp.Description = "Answers questions using information from Elasticsearch";
    genOp.Parameters.Clear();
    genOp.Parameters.Add(new OpenApiParameter
    {
        Name = "topic",
        In = ParameterLocation.Query,
        Required = true,
        Description = "The topic to retrieve the information from. If null or empty, all topics are considered."
    });
    genOp.Parameters.Add(new OpenApiParameter
    {
        Name = "question",
        In = ParameterLocation.Query,
        Required = true,
        Description = "The question to answer"
    });

    return genOp;
});

app.MapGet("/PrivacyPolicy.md", async (HttpRequest req) =>
{
    var mimeType = "text/markdown";
    var fileName = @$"PrivacyPolicy.md";

    var bytes = await File.ReadAllBytesAsync(fileName).ConfigureAwait(false);

    return Results.File(bytes, mimeType, $"{fileName}");
}).ExcludeFromDescription();

app.MapGet("/.well-known/ai-plugin.json", (HttpRequest req) =>
{
    var baseUrl = Environment.GetEnvironmentVariable("VS_TUNNEL_URL");

    var settings = new AIPluginSettings();
    settings.SchemaVersion = "v1";
    settings.NameForHuman = "Elasticsearch Memory Plugin";
    settings.NameForModel = "ElasticMemoryPlugin";
    settings.DescriptionForHuman = "Save information in Elasticsearch indices and then ask questions.";
    settings.DescriptionForModel = "Store information in Elasticsearch and then ask questions about it.";
    settings.Auth = new AIPluginSettings.AuthModel();
    settings.Auth.Type = "none";
    settings.Api = new AIPluginSettings.ApiModel();
    settings.Api.Type = "openapi";
    settings.Api.Url = $"{baseUrl}swagger/v1/swagger.json";
    settings.LogoUrl = "https://avatars.githubusercontent.com/u/89085173?s=200&v=4";
    settings.ContactEmail = "alef@freemindlabs.com";
    settings.LegalInfoUrl = "https://localhost:7106/legal";

    return settings;

    //var json = System.Text.Json.JsonSerializer.Serialize(appSettings.AIPlugin);
    //
    //// replace {url} with the current domain
    //json = json.Replace("{url}", currentDomain, StringComparison.OrdinalIgnoreCase);
    //
    //response.WriteString(json);
    //
    //return response;
}).ExcludeFromDescription(); 

app.Run();

internal record WeatherForecast(DateTime Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(this.TemperatureC / 0.5556);
}
