using System.ComponentModel.DataAnnotations;

namespace AltWirePoint.BusinessLogic.Models.Moderation;

public class BanRequest
{
    [Required]
    public Guid UserId { get; set; }

    [MaxLength(500)]
    public string Reason { get; set; } = string.Empty;

    public DateTime? BannedUntil { get; set; }
}
