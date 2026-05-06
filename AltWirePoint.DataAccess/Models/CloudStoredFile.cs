using AltWirePoint.DataAccess.Enums;
using System.ComponentModel.DataAnnotations.Schema;

namespace AltWirePoint.DataAccess.Models;

public class CloudStoredFile
{
    public Guid Id { get; set; }

    public string Url { get; set; } = null!;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public FileType FileType { get; set; }

    public long FileSize { get; set; }

    [ForeignKey(nameof(Publication))]
    public Guid PublicationId { get; set; }

    public virtual Publication Publication { get; set; } = null!;
}
