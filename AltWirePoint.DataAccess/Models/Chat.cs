using AltWirePoint.DataAccess.Identity;

namespace AltWirePoint.DataAccess.Models;

public class Chat : IKeyedEntity<Guid>
{
    public Guid Id { get; set; }
    
    public string? Name { get; set; } // Optional name for group Chats

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public virtual List<ApplicationUser> Participants { get; set; } = new();

    public virtual List<Message> Messages { get; set; } = new();
}
