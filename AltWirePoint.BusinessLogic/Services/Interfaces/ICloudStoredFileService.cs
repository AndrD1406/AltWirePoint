namespace AltWirePoint.BusinessLogic.Services.Interfaces;

using AltWirePoint.DataAccess.Models;

public interface ICloudStoredFileService
{
    Task InitializeContainersAsync();
    Task<CloudStoredFile> UploadFileAsync(Stream fileStream, string fileName, string contentType, string containerName);
    Task DeleteFileAsync(string fileUrl);
}
