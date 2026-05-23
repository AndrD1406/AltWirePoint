using AltWirePoint.DataAccess.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace AltWirePoint.DataAccess.Models;

public class Message : IKeyedEntity<Guid>
{
    public Guid Id { get; set; }

    public string Content { get; set; } = string.Empty;

    public DateTime SentAt { get; set; } = DateTime.UtcNow;

    public bool IsRead { get; set; }

    [ForeignKey(nameof(ApplicationUser))]
    public Guid SenderId { get; set; }

    public virtual ApplicationUser Sender { get; set; }

    [ForeignKey(nameof(Chat))]
    public Guid ChatId { get; set; }

    public virtual Chat Chat { get; set; }

    public virtual ICollection<CloudStoredFile> Attachments { get; set; } = new List<CloudStoredFile>();
}
