using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FreeMindLabs.KernelMemory.MemoryStorage.Elasticsearch;

public class ElasticsearchConfig
{
    public string Endpoint { get; set; } = null!;
    public string UserName { get; set; } = null!;
    public string Password { get; set; } = null!;
}
