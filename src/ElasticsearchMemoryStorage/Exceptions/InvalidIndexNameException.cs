// Copyright (c) Free Mind Labs, Inc. All rights reserved.

using System;
using System.Collections.Generic;

namespace FreeMindLabs.KernelMemory.Elasticsearch.Exceptions;

/// <summary>
/// Exception thrown when an index name does pass Elasticsearch validation.
/// </summary>
public class InvalidIndexNameException : Exception
{
    /// <inheritdoc/>
    public InvalidIndexNameException(string indexName, IEnumerable<string> errors, Exception? innerException = default)
        : base($"The given index name '{indexName}' is invalid. {string.Join(", ", errors)}", innerException)
    {
        this.IndexName = indexName;
        this.Errors = errors;
    }

    /// <inheritdoc/>
    public InvalidIndexNameException(
        (string IndexName, IEnumerable<string> Errors) conversionResult,
        Exception? innerException = default)

        => (this.IndexName, this.Errors) = conversionResult;

    /// <summary>
    /// The index name that failed validation.
    /// </summary>
    public string IndexName { get; }

    /// <summary>
    /// The list of errors that caused the validation to fail.
    /// </summary>
    public IEnumerable<string> Errors { get; }
}
