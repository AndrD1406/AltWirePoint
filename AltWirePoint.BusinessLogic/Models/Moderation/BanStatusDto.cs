namespace AltWirePoint.BusinessLogic.Models.Moderation;

public class BanStatusDto
{
    public bool IsBanned { get; set; }
    public string? Reason { get; set; }
    public DateTime? BannedAt { get; set; }
    public DateTime? BannedUntil { get; set; }
    public Guid? BannedByUserId { get; set; }
}
