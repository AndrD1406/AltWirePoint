namespace AltWirePoint.BusinessLogic.Models;

public class FileUploadDto
{
    public Stream Content { get; set; } = null!;
    public string FileName { get; set; } = null!;
    public string ContentType { get; set; } = null!;
    public long Length { get; set; }
}
