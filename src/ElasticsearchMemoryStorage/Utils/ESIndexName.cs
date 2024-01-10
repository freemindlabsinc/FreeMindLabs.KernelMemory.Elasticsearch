// Copyright (c) Free Mind Labs, Inc. All rights reserved.

namespace FreeMindLabs.KernelMemory.Elasticsearch.Utils;

public interface IIndexNameHelper
{
    public bool TryConvert(string indexName, out (string ActualIndexName, IEnumerable<string> Errors) result);

    public string Convert(string indexName);
}

/// <inheritdoc />
public class IndexNameHelper : IIndexNameHelper
{
    /// <inheritdoc />
    public IndexNameHelper(ElasticsearchConfig config)
    {
        this.IndexPrefix = config.IndexPrefix;
    }

    /// <summary>
    /// The prefix to use for all index names.
    /// </summary>
    public string IndexPrefix { get; }

    /// <summary>
    /// Attempts to convert the given index name to a valid Elasticsearch index name.
    /// </summary>
    /// <param name="indexName"></param>
    /// <param name="result"></param>
    /// <returns></returns>
    public bool TryConvert(string indexName, out (string ActualIndexName, IEnumerable<string> Errors) result)
    {
        indexName = (this.IndexPrefix + indexName).ToLower();

        // Check for null or whitespace
        if (string.IsNullOrWhiteSpace(indexName))
        {
            result = ("default", Array.Empty<string>());
            return true;
        }

        var errors = new List<string>();

        // Check for invalid start characters
        if (indexName.StartsWith('-') || indexName.StartsWith('_'))
        {
            errors.Add("An index name cannot start with a hyphen (-) or underscore (_).");
        }

        // Check for invalid characters
        if (indexName.Any(x => !char.IsLetterOrDigit(x) && x != '-'))
        {
            errors.Add("An index name can only contain letters, digits, and hyphens (-).");
        }

        // Check for length (max 255 bytes)
        if (System.Text.Encoding.UTF8.GetByteCount(indexName) > 255)
        {
            errors.Add("An index name cannot be longer than 255 bytes.");
        }

        // Avoid names that are dot-only or dot and numbers
        if (indexName.All(c => c == '.' || char.IsDigit(c)))
        {
            errors.Add("Index name cannot be only dots or dots and numbers.");
        }

        if (errors.Count > 0)
        {
            result = (string.Empty, errors);
            return false;
        }

        result = (indexName, Array.Empty<string>());
        return true;
    }


    public string Convert(string indexName)
    {
        if (!TryConvert(indexName, out var result))
        {
            throw new InvalidIndexNameException(result);
        }

        return result.ActualIndexName;
    }
}

/// <summary>
/// Helper methods for Elasticsearch
/// </summary>
public static class xESIndexName
{
    /// <summary>
    /// Attempts to convert the given index name to a valid Elasticsearch index name.
    /// </summary>
    /// <param name="indexName">The index name to convert.</param>
    /// <param name="result">The result of the conversion. The result includes the converted index name if the conversion succeeded, or a list of errors if the conversion failed.</param>
    /// <returns>A structure containing the actual index name or a list of errors if the conversion failed.</returns>
    /// <exception cref="ArgumentException"></exception>
    public static bool TryConvert(string indexName, out (string ActualIndexName, IEnumerable<string> Errors) result)
    {
        // Check for null or whitespace
        if (string.IsNullOrWhiteSpace(indexName))
        {
            result = ("default", Array.Empty<string>());
            return true;
        }

        var errors = new List<string>();

        // Check for invalid start characters
        if (indexName.StartsWith('-') || indexName.StartsWith('_'))
        {
            errors.Add("An index name cannot start with a hyphen (-) or underscore (_).");
        }

        // Check for invalid characters
        if (indexName.Any(x => !char.IsLetterOrDigit(x) && x != '-'))
        {
            errors.Add("An index name can only contain letters, digits, and hyphens (-).");
        }

        // Check for length (max 255 bytes)
        if (System.Text.Encoding.UTF8.GetByteCount(indexName) > 255)
        {
            errors.Add("An index name cannot be longer than 255 bytes.");
        }

        // Avoid names that are dot-only or dot and numbers
        if (indexName.All(c => c == '.' || char.IsDigit(c)))
        {
            errors.Add("Index name cannot be only dots or dots and numbers.");
        }

        if (errors.Count > 0)
        {
            result = (string.Empty, errors);
            return false;
        }

        result = (indexName.ToLower(), Array.Empty<string>());
        return true;
    }

    /// <summary>
    /// Converts the given index name to a valid Elasticsearch index name.
    /// It throws an exception if the conversion fails.
    /// </summary>
    /// <param name="indexName">The index name to convert.</param>
    /// <returns>The converted index name.</returns>
    public static string Convert(string indexName)
    {
        if (!TryConvert(indexName, out var result))
        {
            throw new InvalidIndexNameException(result);
        }

        return result.ActualIndexName;
    }
}
