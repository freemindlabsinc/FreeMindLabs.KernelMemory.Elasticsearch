// Copyright (c) Free Mind Labs, Inc. All rights reserved.

using System;
using System.Collections;
using System.Collections.Generic;
using Elastic.Clients.Elasticsearch.Core.TermVectors;

namespace FreeMindLabs.KernelMemory.Elasticsearch;

/// <summary>
/// TBC
/// </summary>
public static class ElasticsearchMemoryRecordExtensions
{
    public static IEnumerable<Term> ToTerms(this ElasticsearchMemoryRecord record)
    {
        var terms = new List<Term>();

        foreach (var tag in record.Tags)
        {
            //terms.Add(new Term()            
        }

        return terms;
    }
}
