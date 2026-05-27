using AltWirePoint.DataAccess.Models;

namespace AltWirePoint.BusinessLogic.Models;

public class LikeDto
{
    public Guid Id { get; set; }

    public Guid PublicationId { get; set; }

    public string AuthorId { get; set; }

    public string? AuthorName { get; set; }

    public bool IsLiked { get; set; }
}

public static class LikeDtoExtensions
{
    public static LikeDto ToLikeDto(this Like like)
    {
        return new LikeDto
        {
            Id = like.Id,
            PublicationId = like.PublicationId,
            AuthorId = like.AuthorId?.ToString() ?? string.Empty,
            AuthorName = like.Author?.Name,
            IsLiked = like.IsLiked
        };
    }

    public static IEnumerable<LikeDto> ToLikeDtos(this IEnumerable<Like> likes)
    {
        return likes.Select(l => l.ToLikeDto());
    }
}
