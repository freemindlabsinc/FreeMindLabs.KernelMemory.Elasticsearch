// Copyright (c) Free Mind Labs, Inc. All rights reserved.

namespace FreeMindLabs.AI.ElasticsearchMemoryPlugins.FromSk;

using System.Text.Json.Serialization;

//namespace Models;

#pragma warning disable CA1056
#pragma warning disable CA1034

/// <inheritdoc />
public class AIPluginSettings
{
    /// <inheritdoc />
    [JsonPropertyName("schema_version")]
    public string SchemaVersion { get; set; } = "v1";

    /// <inheritdoc />
    [JsonPropertyName("name_for_model")]
    public string NameForModel { get; set; } = string.Empty;

    /// <inheritdoc />
    [JsonPropertyName("name_for_human")]
    public string NameForHuman { get; set; } = string.Empty;

    /// <inheritdoc />
    [JsonPropertyName("description_for_model")]
    public string DescriptionForModel { get; set; } = string.Empty;

    /// <inheritdoc />
    [JsonPropertyName("description_for_human")]
    public string DescriptionForHuman { get; set; } = string.Empty;

    /// <inheritdoc />
    [JsonPropertyName("auth")]
    public AuthModel Auth { get; set; } = new AuthModel();

    /// <inheritdoc />
    [JsonPropertyName("api")]
    public ApiModel Api { get; set; } = new ApiModel();

    /// <inheritdoc />
    [JsonPropertyName("logo_url")]
    public string LogoUrl { get; set; } = string.Empty;

    /// <inheritdoc />
    [JsonPropertyName("contact_email")]
    public string ContactEmail { get; set; } = string.Empty;

    /// <inheritdoc />
    [JsonPropertyName("legal_info_url")]
    public string LegalInfoUrl { get; set; } = string.Empty;

    /// <inheritdoc />
    public class AuthModel
    {
        /// <inheritdoc />
        [JsonPropertyName("type")]
        public string Type { get; set; } = string.Empty;

        /// <inheritdoc />
        [JsonPropertyName("authorization_url")]
        public string AuthorizationType { get; set; } = string.Empty;
    }

    /// <inheritdoc />
    public class ApiModel
    {
        /// <inheritdoc />
        [JsonPropertyName("type")]
        public string Type { get; set; } = "openapi";

        /// <inheritdoc />
        [JsonPropertyName("url")]
        public string Url { get; set; } = string.Empty;

        /// <inheritdoc />
        [JsonPropertyName("has_user_authentication")]
        public bool HasUserAuthentication { get; set; } = false;
    }
}
