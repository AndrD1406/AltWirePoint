using System.ComponentModel.DataAnnotations;

namespace AltWirePoint.BusinessLogic.Models;

public class CommentCreateRequest
{
    [Required]
    public Guid PublicationId { get; set; }

    [Required]
    public Guid AuthorId { get; set; }

    public string? Content { get; set; }

}
