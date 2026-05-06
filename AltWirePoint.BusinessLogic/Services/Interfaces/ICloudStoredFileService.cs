namespace AltWirePoint.BusinessLogic.Services.Interfaces;

using AltWirePoint.DataAccess.Models;

public interface ICloudStoredFileService
{
    Task InitializeContainerAsync();
    Task<CloudStoredFile> UploadFileAsync(Stream fileStream, string fileName, string contentType);
    Task DeleteFileAsync(string fileUrl);
}
