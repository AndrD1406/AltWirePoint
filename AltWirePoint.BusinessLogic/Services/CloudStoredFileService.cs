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
        if (!Uri.TryCreate(fileUrl, UriKind.Absolute, out var uri))
            return;

        var segments = uri.Segments;
        if (segments.Length < 3)
            return;

        var containerName = segments[^2].TrimEnd('/');
        var blobName = segments[^1];

        var containerClient = blobServiceClient.GetBlobContainerClient(containerName);
        var blobClient = containerClient.GetBlobClient(blobName);
        await blobClient.DeleteIfExistsAsync();
    }
}
