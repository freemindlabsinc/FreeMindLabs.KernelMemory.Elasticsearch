// Copyright (c) Free Mind Labs, Inc. All rights reserved.

using System.Collections.Generic;
using System.Linq;
using Microsoft.KernelMemory;

namespace FreeMindLabs.KernelMemory.Elasticsearch.Extensions;

/// <summary>
/// TBC
/// </summary>
public static class MemoryFilterExtensions
{
    /// <summary>
    /// TBC
    /// </summary>
    /// <param name="filter"></param>
    /// <returns></returns>
    public static string ToDebugString(this MemoryFilter? filter)
    {
        if (filter == null)
        {
            return string.Empty;
        }

        // Prints all the tags in the record
        var tags = filter.Select(x => $"({x.Key}={string.Join("|", x.Value)})");
        return string.Join(" & ", tags);
    }

    /// <summary>
    /// TBC
    /// </summary>
    /// <param name="filters"></param>
    /// <returns></returns>
    public static string ToDebugString(this IEnumerable<MemoryFilter?>? filters)
    {
        if (filters == null)
        {
            return string.Empty;
        }

        // Prints all the tags in the record
        var tags = filters.Select(x => x.ToDebugString());
        return string.Join(" & ", tags);
    }
}
