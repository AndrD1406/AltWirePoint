using AltWirePoint.BusinessLogic.Models;
using AltWirePoint.BusinessLogic.Models.Publication;

namespace AltWirePoint.BusinessLogic.Services.Interfaces;

public interface IPublicationService
{
    Task<PublicationDto> Create(PublicationCreateRequest dto, Guid authorId, IEnumerable<FileUploadDto> files);
    Task<PublicationDto> GetById(Guid id);
    Task<PublicationDto> Update(Guid id, PublicationCreateRequest dto);
    Task Delete(Guid id);
    Task<LikeDto> GetLikeById(Guid id);
    Task<IEnumerable<LikeDto>> GetLikesForPublication(Guid publicationId);
    Task<IEnumerable<CommentDto>> GetCommentsForPublication(Guid publicationId);
    Task<int> GetPublicationCountByAuthor(Guid authorId);
    Task<IEnumerable<PublicationDto>> GetReplies(Guid parentId);
    Task<LikeDto> SetLike(Guid publicationId, Guid authorId);
    Task<CommentDto> AddComment(CommentCreateRequest dto, IEnumerable<FileUploadDto> files);
    Task<IEnumerable<PublicationDto>> Get(int skip, int take);
    Task<IEnumerable<PublicationDto>> GetPublicationsByAuthorPaged(Guid authorId, int skip, int take);
    Task<IEnumerable<PublicationDto>> SearchAsync(string query, int skipCount, int maxResultCount);
}
