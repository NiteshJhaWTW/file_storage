using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.DurableTask;
using Microsoft.DurableTask.Client;
using Microsoft.Extensions.Logging;
using Azure.Storage.Blobs;
using Azure.Storage.Files.Shares;
using Castle.Core.Logging;
using file_storage_POC.Models;
using static Azure.Core.HttpHeader;
using FundingCalculation.ProcessOrchestration.Infrastructure;
using file_storage_POC.Enums;
using System.Text;

namespace file_storage_POC
{
    public static class Functions
    {                  
        [Function(nameof(Orchestrator))]
        public static async Task Orchestrator(
            [OrchestrationTrigger] TaskOrchestrationContext context)
        {

            ExportFile exportFile = new ExportFile();
            byte[] content = Encoding.UTF8.GetBytes("Hello World");
            exportFile.FileContent = content;
            exportFile.FileName = "Transaction_20_2024.txt";
            exportFile.FileType = ExportFileType.OnShore;

            await context.CallActivityAsync("StoreFileToBlobStorage", exportFile);

        }

        [Function("StarterFunction")]
        public static async Task<HttpResponseData> StarterFunction(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequestData req,
            [DurableClient] DurableTaskClient client,
            FunctionContext executionContext)
        {
            string instanceId = await client.ScheduleNewOrchestrationInstanceAsync(
                nameof(Orchestrator));

            return client.CreateCheckStatusResponse(req, instanceId);
        }
    }
}
