// Copyright (c) Free Mind Labs, Inc. All rights reserved.

using System;

namespace FreeMindLabs.KernelMemory.Elasticsearch;

/// <summary>
/// Exception thrown when the Elasticsearch configuration is invalid in appSettings, secrets, etc.
/// </summary>
public class ElasticsearchConfigurationException : ElasticsearchException
{
    /// <inheritdoc />
    public ElasticsearchConfigurationException() { }

    /// <inheritdoc />
    public ElasticsearchConfigurationException(string message) : base(message) { }

    /// <inheritdoc />
    public ElasticsearchConfigurationException(string message, Exception? innerException) : base(message, innerException) { }
}
