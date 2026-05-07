using AltWirePoint.DataAccess.Models;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Configuration;
using AltWirePoint.BusinessLogic.Services.Interfaces;

namespace AltWirePoint.BusinessLogic.Services;

public class CloudStoredFileService : ICloudStoredFileService
{
    private readonly BlobServiceClient blobServiceClient;

    public static class ContainerNames
    {
        public const string Publications = "publications";
        public const string ProfilePictures = "profilepictures";
    }

    public CloudStoredFileService(IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("AzureBlobStorage");
        blobServiceClient = new BlobServiceClient(connectionString);
    }

    public async Task InitializeContainersAsync()
    {
        var containerNames = new[] { ContainerNames.Publications, ContainerNames.ProfilePictures };

        foreach (var name in containerNames)
        {
            var containerClient = blobServiceClient.GetBlobContainerClient(name);
            await containerClient.CreateIfNotExistsAsync(PublicAccessType.Blob);
        }
    }

    public async Task<CloudStoredFile> UploadFileAsync(Stream fileStream, string fileName, string contentType, string containerName)
    {
        var containerClient = blobServiceClient.GetBlobContainerClient(containerName);

        await using (fileStream)
        {
            var extension = Path.GetExtension(fileName);
            var fileType = contentType.StartsWith("image") ? DataAccess.Enums.FileType.Image : DataAccess.Enums.FileType.Video;

            string folderPrefix = string.Empty;
            if (containerName == ContainerNames.Publications)
            {
                folderPrefix = fileType == DataAccess.Enums.FileType.Image ? "images/" : "videos/";
            }

            var uniqueFileName = $"{folderPrefix}{Guid.NewGuid()}{extension}";
            var blobClient = containerClient.GetBlobClient(uniqueFileName);

            await blobClient.UploadAsync(fileStream, new BlobHttpHeaders { ContentType = contentType });

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
        if (!Uri.TryCreate(fileUrl, UriKind.Absolute, out var uri))
            return;

        var blobUriBuilder = new BlobUriBuilder(uri);
        var containerClient = blobServiceClient.GetBlobContainerClient(blobUriBuilder.BlobContainerName);
        var blobClient = containerClient.GetBlobClient(blobUriBuilder.BlobName);
        
        await blobClient.DeleteIfExistsAsync();
    }
}
