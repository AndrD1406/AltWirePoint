using AltWirePoint.BusinessLogic.Models;
using AltWirePoint.BusinessLogic.Models.Publication;
using AltWirePoint.BusinessLogic.Services.Interfaces;
using AltWirePoint.DataAccess.Enums;
using AltWirePoint.DataAccess.Models;
using AltWirePoint.DataAccess.Repository.Base;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace AltWirePoint.BusinessLogic.Services;

public class PublicationService : IPublicationService
{
    private readonly IEntityRepository<Guid, Publication> publicationRepository;
    private readonly IEntityRepository<Guid, Like> likeRepository;
    private readonly ICloudStoredFileService cloudStoredFileService;

    public PublicationService(
        IEntityRepository<Guid, Publication> publicationRepository,
        IEntityRepository<Guid, Like> likeRepository,
        ICloudStoredFileService cloudStoredFileService)
    {
        this.publicationRepository = publicationRepository;
        this.likeRepository = likeRepository;
        this.cloudStoredFileService = cloudStoredFileService;
    }

    public async Task<PublicationDto> Create(PublicationCreateRequest request, Guid authorId, IEnumerable<FileUploadDto> files)
    {
        var publication = request.ToPublication();
        publication.AuthorId = authorId;
        publication.CreatedAt = DateTime.UtcNow;
        publication.CloudStoredFiles = new List<CloudStoredFile>();

        if (files != null)
        {
            var uploadTasks = files.Select(async file =>
            {
                var storedFile = await cloudStoredFileService.UploadFileAsync(file.Content, file.FileName, file.ContentType, CloudStoredFileService.ContainerNames.Publications);
                return storedFile;
            });

            var storedFiles = await Task.WhenAll(uploadTasks);
            publication.CloudStoredFiles.AddRange(storedFiles);
        }

        var created = await publicationRepository.Create(publication);
        return created.ToPublicationDto();
    }

    public async Task<PublicationDto> GetById(Guid id)
    {
        var publication = await publicationRepository.GetByIdWithDetails(id, 
            includeProperties: "Author.ProfilePicture,CloudStoredFiles");
        return publication.ToPublicationDto();
    }

    public async Task<LikeDto> GetLikeById(Guid id)
    {
        var like = await likeRepository.GetById(id);
        return like.ToLikeDto();
    }

    public async Task<IEnumerable<LikeDto>> GetLikesForPublication(Guid publicationId)
    {
        var likes = await likeRepository
            .GetByFilter(l => l.PublicationId == publicationId).ToListAsync();
        return likes.ToLikeDtos();
    }

    public async Task<IEnumerable<CommentDto>> GetCommentsForPublication(Guid publicationId, int skip, int take, Guid currentUserId)
    {
        var comments = await publicationRepository
            .Get(skip, take, 
                 includeProperties: "Author.ProfilePicture,CloudStoredFiles", 
                 whereExpression: p => p.ParentId == publicationId,
                 orderBy: new List<(Expression<Func<Publication, object>>, SortDirection)>
                 {
                     (p => p.CreatedAt, SortDirection.Ascending)
                 })
            .AsNoTracking().ToListAsync();

        var enriched = await MapWithLikesAndComments(comments, currentUserId);

        return enriched.Select(dto =>
        {
            var entity = comments.First(c => c.Id == dto.Id);
            return new CommentDto
            {
                Id = dto.Id,
                ParentId = entity.ParentId!.Value,
                Description = dto.Description,
                FileUrls = dto.FileUrls,
                CreatedAt = dto.CreatedAt,
                AuthorId = dto.AuthorId,
                AuthorName = dto.AuthorName,
                AuthorProfilePictureUrl = dto.AuthorProfilePictureUrl,
                LikeCount = dto.LikeCount,
                CommentCount = dto.CommentCount,
                IsLikedByCurrentUser = dto.IsLikedByCurrentUser
            };
        });
    }

    public async Task<int> GetPublicationCountByAuthor(Guid authorId)
    {
        return await publicationRepository.Count(p => p.AuthorId == authorId && p.ParentId == null);
    }

    public async Task<IEnumerable<PublicationDto>> GetReplies(Guid parentId)
    {
        var replies = await publicationRepository
            .GetByFilter(p => p.ParentId == parentId)
            .AsNoTracking().ToListAsync();
        return replies.Select(r => r.ToPublicationDto());
    }

    public async Task<PublicationDto> Update(Guid id, PublicationCreateRequest dto)
    {
        var existing = await publicationRepository.GetById(id);
        dto.ApplyTo(existing);
        var updated = await publicationRepository.Update(existing);
        return updated.ToPublicationDto();
    }

    public async Task Delete(Guid id)
    {
        var publicationToDelete = await this.publicationRepository.GetById(id);
        
        if (publicationToDelete.CloudStoredFiles != null && publicationToDelete.CloudStoredFiles.Any())
        {
            foreach (var file in publicationToDelete.CloudStoredFiles)
            {
                await cloudStoredFileService.DeleteFileAsync(file.Url);
            }
        }

        await publicationRepository.Delete(publicationToDelete);
    }

    public async Task<LikeDto> SetLike(Guid publicationId, Guid authorId)
    {
        var existing = (await likeRepository.Get().ToListAsync())
            .FirstOrDefault(l
                => l.PublicationId == publicationId
                && l.AuthorId == authorId);

        if (existing != null)
        {
            existing.IsLiked = !existing.IsLiked;
            var updated = await likeRepository.Update(existing);
            return updated.ToLikeDto();
        }

        var like = new Like
        {
            PublicationId = publicationId,
            AuthorId = authorId,
            IsLiked = true
        };

        var created = await likeRepository.Create(like);
        return created.ToLikeDto();
    }

    public async Task<IEnumerable<LikeDto>> GetAllLikes()
    => (await likeRepository.Get().ToListAsync()).ToLikeDtos();

    public async Task<CommentDto> CreateComment(CommentCreateRequest request, IEnumerable<FileUploadDto> files)
    {
        var commentEntity = request.ToPublication();
        commentEntity.CloudStoredFiles = new List<CloudStoredFile>();

        if (files != null)
        {
            var uploadTasks = files.Select(async file =>
            {
                var storedFile = await cloudStoredFileService.UploadFileAsync(file.Content, file.FileName, file.ContentType, CloudStoredFileService.ContainerNames.Publications);
                storedFile.PublicationId = commentEntity.Id;
                return storedFile;
            });

            var storedFiles = await Task.WhenAll(uploadTasks);
            commentEntity.CloudStoredFiles.AddRange(storedFiles);
        }

        var created = await publicationRepository.Create(commentEntity);

        var createdWithIncludes = await publicationRepository
            .GetByIdWithDetails(created.Id, includeProperties: "Author.ProfilePicture,CloudStoredFiles");

        return createdWithIncludes!.ToCommentDto();
    }

    public async Task<IEnumerable<PublicationDto>> Get(int skip, int take, Guid currentUserId)
    {
        var pageEntities = await publicationRepository
            .Get(
                skip: skip,
                take: take,
                includeProperties: "Author.ProfilePicture,CloudStoredFiles",
                whereExpression: p => p.ParentId == null,
                orderBy: new List<(Expression<Func<Publication, object>>, SortDirection)>
                {
                    (p => p.CreatedAt, SortDirection.Descending)
                }
            )
            .AsNoTracking()
            .ToListAsync();

        return await MapWithLikesAndComments(pageEntities, currentUserId);
    }

    public async Task<IEnumerable<PublicationDto>> GetPublicationsByAuthorPaged(
        Guid authorId, int skip, int take, Guid currentUserId)
    {
        var pageEntities = await publicationRepository
            .Get(
                skip: skip,
                take: take,
                includeProperties: "Author.ProfilePicture,CloudStoredFiles",
                whereExpression: p => p.ParentId == null && p.AuthorId == authorId,
                orderBy: new List<(Expression<Func<Publication, object>>, SortDirection)>
                {
                    (p => p.CreatedAt, SortDirection.Descending)
                }
            )
            .AsNoTracking()
            .ToListAsync();

        return await MapWithLikesAndComments(pageEntities, currentUserId);
    }

    public async Task<IEnumerable<PublicationDto>> SearchAsync(string query, int skipCount, int maxResultCount, Guid currentUserId)
    {
        var searchPattern = $"%{query}";

        var entities = await publicationRepository
            .Get(
                skip: skipCount,
                take: maxResultCount,
                includeProperties: "Author.ProfilePicture,CloudStoredFiles",
                whereExpression: p => p.Description != null && EF.Functions.Like(p.Description, searchPattern),
                orderBy: new List<(Expression<Func<Publication, object>>, SortDirection)>
                {
                    (p => p.CreatedAt, SortDirection.Descending)
                }
            )
            .AsNoTracking()
            .ToListAsync();

        return await MapWithLikesAndComments(entities, currentUserId);
    }

    private async Task<IEnumerable<PublicationDto>> MapWithLikesAndComments(
    List<Publication> pageEntities, Guid currentUserId)
    {
        var dtos = pageEntities.Select(p => p.ToPublicationDto()).ToList();
        var publicationIds = pageEntities.Select(p => p.Id).ToList();

        var likeCounts = await likeRepository
            .GetByFilter(l => publicationIds.Contains(l.PublicationId) && l.IsLiked)
            .GroupBy(l => l.PublicationId)
            .Select(g => new { PublicationId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.PublicationId, x => x.Count);

        var userLikes = await likeRepository
            .GetByFilter(l => publicationIds.Contains(l.PublicationId) && l.AuthorId == currentUserId && l.IsLiked)
            .Select(l => l.PublicationId)
            .ToListAsync();

        var commentCounts = await publicationRepository
            .GetByFilter(p => p.ParentId.HasValue && publicationIds.Contains(p.ParentId.Value))
            .GroupBy(p => p.ParentId.Value)
            .Select(g => new { ParentId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.ParentId, x => x.Count);

        foreach (var dto in dtos)
        {
            dto.LikeCount = likeCounts.GetValueOrDefault(dto.Id, 0);
            dto.CommentCount = commentCounts.GetValueOrDefault(dto.Id, 0);
            dto.IsLikedByCurrentUser = userLikes.Contains(dto.Id);
        }

        return dtos;
    }
}


