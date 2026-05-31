using AltWirePoint.DataAccess.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AltWirePoint.DataAccess.Models;

public class UserFollow : IKeyedEntity<Guid>
{
    [Key]
    public Guid Id { get; set; }

    [ForeignKey(nameof(Follower))]
    public Guid FollowerId { get; set; }

    public virtual ApplicationUser? Follower { get; set; }

    [ForeignKey(nameof(Followed))]
    public Guid FollowedId { get; set; }

    public virtual ApplicationUser? Followed { get; set; }

    public DateTime FollowedAt { get; set; } = DateTime.UtcNow;
}
