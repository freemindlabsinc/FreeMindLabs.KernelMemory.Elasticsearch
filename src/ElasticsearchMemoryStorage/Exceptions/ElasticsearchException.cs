// Copyright (c) Free Mind Labs, Inc. All rights reserved.

using Microsoft.KernelMemory;

// TODO: I am not using the normal ns name given the folder... should I?
namespace FreeMindLabs.KernelMemory.Elasticsearch;

/// <summary>
/// Base exception for all the exceptions thrown by the Elasticsearch connector for KernelMemory
/// </summary>
public class ElasticsearchException : KernelMemoryException
{
    /// <inheritdoc />
    public ElasticsearchException() { }

    /// <inheritdoc />
    public ElasticsearchException(string message) : base(message) { }

    /// <inheritdoc />
    public ElasticsearchException(string message, Exception? innerException) : base(message, innerException) { }
}
