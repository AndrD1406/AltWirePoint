using AltWirePoint.DataAccess.Models;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Configuration;
using AltWirePoint.BusinessLogic.Services.Interfaces;

namespace AltWirePoint.BusinessLogic.Services;

public class CloudStoredFileService : ICloudStoredFileService
{
    private readonly BlobContainerClient containerClient;
    private const string ContainerName = "publications";

    public CloudStoredFileService(IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("AzureBlobStorage");
        var blobServiceClient = new BlobServiceClient(connectionString);
        containerClient = blobServiceClient.GetBlobContainerClient(ContainerName);
    }

    public async Task InitializeContainerAsync()
    {
        await containerClient.CreateIfNotExistsAsync(PublicAccessType.Blob);
    }

    public async Task<CloudStoredFile> UploadFileAsync(Stream fileStream, string fileName, string contentType)
    {
        await InitializeContainerAsync();

        await using (fileStream)
        {
            var extension = Path.GetExtension(fileName);
            var uniqueFileName = $"{Guid.NewGuid()}{extension}";
            var blobClient = containerClient.GetBlobClient(uniqueFileName);

            await blobClient.UploadAsync(fileStream, new BlobHttpHeaders { ContentType = contentType });

            var fileType = contentType.StartsWith("image") ? DataAccess.Enums.FileType.Image : DataAccess.Enums.FileType.Video;

            return new CloudStoredFile
            {
                Id = Guid.NewGuid(),
                Url = blobClient.Uri.ToString(),
                FileType = fileType,
                FileSize = fileStream.Length,
                CreatedAt = DateTime.UtcNow
            };
        }
    }

    public async Task DeleteFileAsync(string fileUrl)
    {
        if (Uri.TryCreate(fileUrl, UriKind.Absolute, out var uri))
        {
            var fileName = Path.GetFileName(uri.LocalPath);
            var blobClient = containerClient.GetBlobClient(fileName);
            await blobClient.DeleteIfExistsAsync();
        }
    }
}
