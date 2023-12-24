// Copyright (c) Free Mind Labs, Inc. All rights reserved.

using System.Reflection;
using Elastic.Clients.Elasticsearch;
using FreeMindLabs.KernelMemory.Elasticsearch;

namespace UnitTests;
internal static class ElastictestsHelper
{
    /// <summary>
    /// Deletes all indices that are created by the test methods of the given class.
    /// Indices must have the same name of their test method.
    /// </summary>
    public static async Task DeleteIndicesOfTestAsync<T>(this ElasticsearchClient client)
    {
        // iterates thru all method names of the test class and deletes the indice with the same name
        var methods = typeof(T).GetMethods(BindingFlags.Public | BindingFlags.Instance)
                               .Where(m =>
                                    (m.GetCustomAttribute<Xunit.FactAttribute>() != null)
                                    ||
                                    (m.GetCustomAttribute<Xunit.TheoryAttribute>() != null)
                               )
                               .ToArray();
        if (methods.Length == 0)
        {
            throw new ArgumentException($"No public test methods found in class '{typeof(T).Name}'.");
        }

        foreach (var method in methods)
        {
            var indexName = Indexname.Convert(method.Name);
            var delResp = await client.Indices.DeleteAsync(indices: indexName)
                                      .ConfigureAwait(false);

            if (delResp.IsSuccess())
            {
                Console.WriteLine($"Deleted index '{indexName}'.");
            }
        }

    }
}
