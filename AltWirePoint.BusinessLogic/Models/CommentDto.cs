using AltWirePoint.BusinessLogic.Models.Publication;

namespace AltWirePoint.BusinessLogic.Models;

public class CommentDto : PublicationDto
{
    public Guid ParentId { get; set; }
}

public static class CommentDtoExtensions
{
    public static CommentDto ToCommentDto(this DataAccess.Models.Publication publication)
    {
        return new CommentDto
        {
            Id = publication.Id,
            ParentId = publication.ParentId!.Value,
            CreatedAt = publication.CreatedAt,
            Description = publication.Description ?? string.Empty,
            AuthorId = publication.AuthorId,
            AuthorName = publication.Author!.UserName,
            AuthorProfilePictureUrl = publication.Author?.ProfilePicture?.Url,
            FileUrls = publication.CloudStoredFiles?.Select(f => f.Url).ToList() ?? new List<string>()
        };
    }
}