namespace AltWirePoint.BusinessLogic.Models.Publication;

public class PublicationDto
{
    public Guid Id { get; set; }

    public string? Description { get; set; }

    public List<string>? FileUrls { get; set; }

    public DateTime CreatedAt { get; set; }

    public Guid AuthorId { get; set; }

    public string? AuthorName { get; set; }

    public string? AuthorLogo { get; set; }

    public List<LikeDto>? Likes { get; set; }

    public List<CommentDto>? Comments { get; set; }
}
