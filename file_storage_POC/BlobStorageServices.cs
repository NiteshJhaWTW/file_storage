using Azure;
using Azure.Storage.Blobs;
using file_storage_POC.Enums;
using file_storage_POC.Models;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FundingCalculation.ProcessOrchestration.Infrastructure
{
    /// <summary>
    /// Class which provides the functionality to store the files to Azure blob Storage.
    /// </summary>
    public class BlobStorageServices
    {
        private readonly ILogger _logger;
        private readonly BlobContainerClient _blobContainerClient;
        private readonly AsyncRetryPolicy _retryPolicy;


        /// <summary>
        /// Constructor to initialize objects.
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="blobContainerClient" cref="BlobContainerClient"></param>
        public BlobStorageServices(ILogger<BlobStorageServices> logger
            , BlobContainerClient blobContainerClient
            , AsyncRetryPolicy retryPolicy)
        {
            _logger = logger;
            _blobContainerClient = blobContainerClient;
            _retryPolicy = retryPolicy;
        }


        /// <summary>
        /// This function uploads the file it recieves as parameter to Azure blob Storage.
        /// </summary>
        /// <param name="exportFile"> object that stores file content and other properties</param>
        /// <param name="cancellationToken"></param>
        /// <returns>Function doesn't return anything.</returns>
        public async Task UploadAsync(ExportFile exportFile)
        {

            string blobName = GetBlobPath(exportFile.FileType, exportFile.FileName);
            var blobClient = _blobContainerClient.GetBlobClient(blobName);


            await _retryPolicy.ExecuteAsync(async () =>
            {
                try
                {
                    using (MemoryStream stream = new MemoryStream(exportFile.FileContent))
                    {
                        await blobClient.UploadAsync(stream, true);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Failed to upload blob :{ex.Message}");
                    throw;
                }

            });

        }

        /// <summary>
        /// This function returns the blobName where file would be stored depending on the file type.
        /// </summary>
        /// <param name="fileType">reprsents the file is an offshore or an onshore file</param>
        /// <param name="fileName">reprsents file name</param>
        /// <returns>Function returns blobName.</returns>
        private string GetBlobPath(ExportFileType fileType, string fileName)
        {
            string offshoreFolder = "Acclaris", onshoreFolder = "Acclaris Onshore-Only";
            string blobName;
            if (fileType == ExportFileType.OffShore)
            {
                blobName = offshoreFolder + "/" + fileName;
            }
            else
            {
                blobName = onshoreFolder + "/" + fileName;
            }

            return blobName;
        }
    }
}
