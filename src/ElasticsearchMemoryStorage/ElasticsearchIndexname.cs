// Copyright (c) Free Mind Labs, Inc. All rights reserved.

using System;
using System.Linq;

namespace FreeMindLabs.KernelMemory.Elasticsearch;

/// <summary>
/// Helper methods for Elasticsearch
/// </summary>
public static class ElasticsearchIndexname
{
    /// <summary>
    /// Validates an Elasticsearch index name.
    /// </summary>
    /// <param name="indexName"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1304:Specify CultureInfo", Justification = "<Pending>")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1311:Specify a culture or use an invariant version", Justification = "<Pending>")]
    public static string Validate(string indexName)
    {
        // Check for null or whitespace
        if (string.IsNullOrWhiteSpace(indexName))
        {
            return "default";
        }

        // Convert to lowercase (Elasticsearch index names must be lowercase)
        indexName = indexName.ToLower();

        // Check for invalid start characters
        if (indexName.StartsWith('-') || indexName.StartsWith('_'))
        {
            throw new ArgumentException("Index name cannot start with a hyphen (-) or underscore (_).", nameof(indexName));
        }

        // Check for invalid characters
        if (indexName.Any(x => !char.IsLetterOrDigit(x) && x != '-'))
        {
            throw new ArgumentException("Index name can only contain letters, digits, and hyphens (-).", nameof(indexName));
        }

        // Check for length (max 255 bytes)
        if (System.Text.Encoding.UTF8.GetByteCount(indexName) > 255)
        {
            throw new ArgumentException("Index name cannot be longer than 255 bytes.", nameof(indexName));
        }

        // Avoid names that are dot-only or dot and numbers
        if (indexName.All(c => c == '.' || char.IsDigit(c)))
        {
            throw new ArgumentException("Index name cannot be only dots or dots and numbers.", nameof(indexName));
        }

        return indexName;
    }

}
