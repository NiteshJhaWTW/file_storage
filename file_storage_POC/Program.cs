using Azure.Storage.Blobs;
using Azure;
using FundingCalculation.ProcessOrchestration.Infrastructure;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Polly;
using Azure.Identity;

var host = new HostBuilder()
    .ConfigureFunctionsWebApplication()
    .ConfigureServices(services =>
    {
        services.AddApplicationInsightsTelemetryWorkerService();
        services.ConfigureFunctionsApplicationInsights();
        services.Configure<KestrelServerOptions>(options =>
        {
            options.AllowSynchronousIO = true;
        });


        //var connStringStorageAccount = Environment.GetEnvironmentVariable("AzureWebJobsStorage");

        var storageAccountName = Environment.GetEnvironmentVariable("StorageAccountName");
        var managedIdentityClientId = Environment.GetEnvironmentVariable("managedIdentityClientID");
        DefaultAzureCredentialOptions options = new DefaultAzureCredentialOptions { ManagedIdentityClientId = managedIdentityClientId };
        DefaultAzureCredential credential = new DefaultAzureCredential(options);

        BlobServiceClient blobServiceClient = new BlobServiceClient(new Uri($"https://{storageAccountName}.blob.core.windows.net"),credential);

        //string connstring = Environment.GetEnvironmentVariable("AzureWebJobsStorage");
        //BlobServiceClient blobServiceClient = new BlobServiceClient(connstring);
        services.AddSingleton(blobServiceClient);

        var rootContainerName = Environment.GetEnvironmentVariable("ContainerName");
        var blobContainerClient = blobServiceClient.GetBlobContainerClient(rootContainerName);

        services.AddScoped<BlobContainerClient>(c => blobContainerClient);

        services.AddScoped<BlobStorageServices>();

        var retryPolicy = Policy
                        .Handle<RequestFailedException>()
                        .Or<IOException>()
                        .Or<TimeoutException>()
                        .WaitAndRetryAsync(
                                retryCount: 5,
                                sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                                onRetryAsync: async (exception, timespan, retryAttempt, _) =>
                                {
                                    Console.WriteLine($"Retrying attempt {retryAttempt} due to {exception.Message}." +
                                        $"Next retry in {timespan.TotalSeconds} seconds.");
                                });

        services.AddSingleton(retryPolicy);
    })
    .Build();

host.Run();
