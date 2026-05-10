namespace AltWirePoint.BusinessLogic.Models.Publication;

public class PublicationDto
{
    public Guid Id { get; set; }

    public string? Description { get; set; }

    public List<string>? FileUrls { get; set; }

    public DateTime CreatedAt { get; set; }

    public Guid AuthorId { get; set; }

    public string? AuthorName { get; set; }

    public string? AuthorProfilePictureUrl { get; set; }

    public int LikeCount { get; set; }

    public int CommentCount { get; set; }

    public bool IsLikedByCurrentUser { get; set; }
}

public static class PublicationDtoExtensions
{
    public static PublicationDto ToPublicationDto(this DataAccess.Models.Publication publication)
    {
        return new PublicationDto
        {
            Id = publication.Id,
            Description = publication.Description,
            CreatedAt = publication.CreatedAt,
            AuthorId = publication.AuthorId,
            FileUrls = publication.CloudStoredFiles?.Select(f => f.Url).ToList(),
            AuthorName = publication.Author?.Name,
            AuthorProfilePictureUrl = publication.Author?.ProfilePicture?.Url
        };
    }
}
