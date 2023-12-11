// Copyright (c) Free Mind Labs, Inc. All rights reserved.

namespace FreeMindLabs.KernelMemory.Elasticsearch;

/// <summary>
/// TBC
/// </summary>
public class ElasticsearchConfig
{
    /// <summary>
    /// The default Elasticsearch endpoint.
    /// </summary>
    public static readonly string DefaultEndpoint = "https://localhost:9200";

    /// <summary>
    /// The name of the section that will contain the configuration for Elasticsearch
    /// (e.g. appSettings.json, user secrets, etc.).
    /// </summary>
    public const string DefaultSettingsSection = "Elasticsearch";

    /// <summary>
    /// TBC
    /// </summary>
    /// <param name="endpoint"></param>
    /// <param name="userName"></param>
    /// <param name="password"></param>
    /// <param name="certificateFingerPrint"></param>
    public ElasticsearchConfig(
        string endpoint,
        string userName,
        string password,
        string certificateFingerPrint)
    {
        this.Endpoint = endpoint;
        this.UserName = userName;
        this.Password = password;
        this.CertificateFingerPrint = certificateFingerPrint;
    }

    /// <summary>
    /// TBC
    /// </summary>
    public string CertificateFingerPrint { get; init; }

    /// <summary>
    /// TBC
    /// </summary>
    public string Endpoint { get; }

    /// <summary>
    /// TBC
    /// </summary>
    public string UserName { get; }

    /// <summary>
    /// TBC
    /// </summary>
    public string Password { get; }
}
