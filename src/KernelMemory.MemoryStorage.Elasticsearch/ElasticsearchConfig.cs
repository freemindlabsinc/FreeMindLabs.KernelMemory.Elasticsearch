// Copyright (c) Free Mind Labs, Inc. All rights reserved.

namespace FreeMindLabs.KernelMemory.MemoryStorage.Elasticsearch;

public class ElasticsearchConfig
{
    public string CertificateFingerPrint { get; set; } = null!;
    public string Endpoint { get; set; } = null!;
    public string UserName { get; set; } = null!;
    public string Password { get; set; } = null!;

}
