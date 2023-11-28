
// See https://aka.ms/new-console-template for more information
Console.WriteLine("Hello, World!");

var memory = new KernelMemoryBuilder();
//    .WithOpenAIDefaults(Env.Var("OPENAI_API_KEY"))
//    // .FromAppSettings() => read "KernelMemory" settings from appsettings.json (if available), see https://github.com/microsoft/kernel-memory/blob/main/dotnet/Service/appsettings.json as an example
//    // .WithAzureBlobsStorage(new AzureBlobsConfig {...})                                              => use Azure Blobs
//    // .WithAzureCognitiveSearch(Env.Var("ACS_ENDPOINT"), Env.Var("ACS_API_KEY"))                      => use Azure Cognitive Search
//    // .WithQdrant("http://127.0.0.1:6333")                                                            => use Qdrant docker
//    // .WithAzureFormRecognizer(Env.Var("AZURE_COG_SVCS_ENDPOINT"), Env.Var("AZURE_COG_SVCS_API_KEY")) => use Azure Form Recognizer OCR
//    .Build<MemoryServerless>();