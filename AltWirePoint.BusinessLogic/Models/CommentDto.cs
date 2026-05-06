namespace AltWirePoint.BusinessLogic.Models;

public class CommentDto
{
    public Guid Id { get; set; }

    public Guid ParentId { get; set; }

    public DateTime CreatedAt { get; set; }

    public List<string>? FileUrls { get; set; }

    public string Content { get; set; }

    public Guid AuthorId { get; set; }

    public string? AuthorName { get; set; }

    public string? AuthorLogo { get; set; }
}
