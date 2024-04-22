using Castle.Core.Logging;
using file_storage_POC.Models;
using FundingCalculation.ProcessOrchestration.Infrastructure;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace file_storage_POC
{
    public class Activity
    {
        private readonly ILogger<Activity> _logger;
        private readonly BlobStorageServices _storageServices;

        public Activity(ILogger<Activity> logger, BlobStorageServices storageServices)
        {
            _logger = logger;
            _storageServices = storageServices;
        }

        [Function(nameof(StoreFileToBlobStorage))]
        public async Task StoreFileToBlobStorage([ActivityTrigger] ExportFile exportFile, FunctionContext executionContext)
        {
            await _storageServices.UploadAsync(exportFile);
        }
    }
}

