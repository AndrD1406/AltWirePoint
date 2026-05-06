using AltWirePoint.DataAccess.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AltWirePoint.DataAccess.Models;

public class Publication : IKeyedEntity<Guid>
{
    [Key]
    public Guid Id { get; set; }

    public string? Description { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [ForeignKey(nameof(ApplicationUser))]
    public Guid AuthorId { get; set; }

    public virtual ApplicationUser? Author { get; set; }

    public virtual List<Like>? Likes { get; set; }

    public Guid? ParentId { get; set; } = null;

    [ForeignKey(nameof(ParentId))]
    public virtual Publication? Parent { get; set; }

    public virtual List<Publication> Comments { get; set; } = new List<Publication>();

    public virtual List<CloudStoredFile>? CloudStoredFiles { get; set; }
}
