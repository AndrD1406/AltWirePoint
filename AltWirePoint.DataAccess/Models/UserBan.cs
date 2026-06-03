using AltWirePoint.DataAccess.Identity;

namespace AltWirePoint.DataAccess.Models;

public class UserBan : IKeyedEntity<long>
{
    public long Id { get; set; }
    
    public Guid UserId { get; set; }
    
    public virtual ApplicationUser User { get; set; } = null!;
    
    public Guid BannedByUserId { get; set; }
    
    public virtual ApplicationUser BannedByUser { get; set; } = null!;
    
    public DateTime BannedAt { get; set; }
    
    public DateTime? BannedUntil { get; set; }
    
    public string Reason { get; set; } = string.Empty;
    
    public bool IsActive { get; set; } = true;
}
