using AltWirePoint.DataAccess.Models;
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

public static class CommentCreateRequestExtensions
{
    public static DataAccess.Models.Publication ToPublication(this CommentCreateRequest request)
    {
        return new DataAccess.Models.Publication
        {
            AuthorId = request.AuthorId,
            ParentId = request.PublicationId,
            Description = request.Content,
            CreatedAt = DateTime.UtcNow
        };
    }
}
