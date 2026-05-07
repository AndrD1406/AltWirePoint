using AltWirePoint.BusinessLogic.Models;
using AltWirePoint.BusinessLogic.Models.Publication;
using AltWirePoint.BusinessLogic.Services.Interfaces;
using AltWirePoint.DataAccess.Enums;
using AltWirePoint.DataAccess.Models;
using AltWirePoint.DataAccess.Repository.Base;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace AltWirePoint.BusinessLogic.Services;

public class PublicationService : IPublicationService
{
    private readonly IEntityRepository<Guid, Publication> publicationRepository;
    private readonly IEntityRepository<Guid, Like> likeRepository;
    private readonly ICloudStoredFileService cloudStoredFileService;
    private readonly IMapper mapper;

    public PublicationService(
        IEntityRepository<Guid, Publication> publicationRepository,
        IEntityRepository<Guid, Like> likeRepository,
        ICloudStoredFileService cloudStoredFileService,
        IMapper mapper)
    {
        this.publicationRepository = publicationRepository;
        this.likeRepository = likeRepository;
        this.cloudStoredFileService = cloudStoredFileService;
        this.mapper = mapper;
    }

    public async Task<PublicationDto> Create(PublicationCreateRequest request, Guid authorId, IEnumerable<FileUploadDto> files)
    {
        var publication = mapper.Map<Publication>(request);
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
        return mapper.Map<PublicationDto>(created);
    }

    public async Task<PublicationDto> GetById(Guid id)
    {
        var publication = await publicationRepository.GetById(id);
        return mapper.Map<PublicationDto>(publication);
    }

    public async Task<LikeDto> GetLikeById(Guid id)
    {
        var like = await likeRepository.GetById(id);
        return mapper.Map<LikeDto>(like);
    }

    public async Task<IEnumerable<LikeDto>> GetLikesForPublication(Guid publicationId)
    {
        var likes = await likeRepository
            .GetByFilter(l => l.PublicationId == publicationId).ToListAsync();
        return mapper.Map<IEnumerable<LikeDto>>(likes);
    }

    public async Task<IEnumerable<CommentDto>> GetCommentsForPublication(Guid publicationId)
    {
        var comments = await publicationRepository
            .GetByFilter(p => p.ParentId == publicationId)
            .AsNoTracking().ToListAsync();
        return mapper.Map<IEnumerable<CommentDto>>(comments);
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
        return mapper.Map<IEnumerable<PublicationDto>>(replies);
    }

    public async Task<PublicationDto> Update(Guid id, PublicationCreateRequest dto)
    {
        var existing = await publicationRepository.GetById(id);
        mapper.Map(dto, existing);
        var updated = await publicationRepository.Update(existing);
        return mapper.Map<PublicationDto>(updated);
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
            return mapper.Map<LikeDto>(updated);
        }

        var like = new Like
        {
            PublicationId = publicationId,
            AuthorId = authorId,
            IsLiked = true
        };

        var created = await likeRepository.Create(like);
        return mapper.Map<LikeDto>(created);
    }

    public async Task<IEnumerable<LikeDto>> GetAllLikes()
    => mapper.Map<IEnumerable<LikeDto>>(await likeRepository.Get().ToListAsync());

    public async Task<CommentDto> AddComment(CommentCreateRequest request, IEnumerable<FileUploadDto> files)
    {
        var commentEntity = mapper.Map<Publication>(request);
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

        return mapper.Map<CommentDto>(created);
    }

    public async Task<IEnumerable<PublicationDto>> Get(int skip, int take)
    {
        var pageEntities = await publicationRepository
            .Get(
                skip: skip,
                take: take,
                includeProperties: $"{nameof(Publication.Author)},{nameof(Publication.CloudStoredFiles)}",
                whereExpression: p => p.ParentId == null,
                orderBy: new List<(Expression<Func<Publication, object>>, SortDirection)>
                {
                    (p => p.CreatedAt, SortDirection.Descending)
                }
            )
            .AsNoTracking()
            .ToListAsync();

        return await MapWithLikesAndComments(pageEntities);
    }

    public async Task<IEnumerable<PublicationDto>> GetPublicationsByAuthorPaged(
        Guid authorId, int skip, int take)
    {
        var pageEntities = await publicationRepository
            .Get(
                skip: skip,
                take: take,
                includeProperties: $"{nameof(Publication.Author)},{nameof(Publication.CloudStoredFiles)}",
                whereExpression: p => p.ParentId == null && p.AuthorId == authorId,
                orderBy: new List<(Expression<Func<Publication, object>>, SortDirection)>
                {
                    (p => p.CreatedAt, SortDirection.Descending)
                }
            )
            .AsNoTracking()
            .ToListAsync();

        return await MapWithLikesAndComments(pageEntities);
    }

    public async Task<IEnumerable<PublicationDto>> SearchAsync(string query, int skipCount, int maxResultCount)
    {
        var searchPattern = $"%{query}";

        var entities = await publicationRepository
            .Get(
                skip: skipCount,
                take: maxResultCount,
                includeProperties: nameof(Publication.CloudStoredFiles),
                whereExpression: p => p.Description != null && EF.Functions.ILike(p.Description, searchPattern),
                orderBy: new List<(Expression<Func<Publication, object>>, SortDirection)>
                {
                    (p => p.CreatedAt, SortDirection.Descending)
                }
            )
            .AsNoTracking()
            .ToListAsync();

        return entities
            .Select(p => mapper.Map<PublicationDto>(p));
    }

    private async Task<IEnumerable<PublicationDto>> MapWithLikesAndComments(
        List<Publication> pageEntities)
    {
        // Map basic publication details first
        var dtos = pageEntities.Select(mapper.Map<PublicationDto>).ToList();

        // Get all publication IDs for the current page
        var publicationIds = pageEntities.Select(p => p.Id).ToList();

        // Get all likes for the publications in the current page
        var relatedLikes = await likeRepository
            .GetByFilter(l => publicationIds.Contains(l.PublicationId))
            .AsNoTracking()
            .ToListAsync();

        // Map likes to a lookup for quick access
        var allLikesLookup = relatedLikes
            .Select(mapper.Map<LikeDto>)
            .ToLookup(l => l.PublicationId);

        // Get all comments for the publications in the current page
        var relatedComments = await publicationRepository
            .GetByFilter(p => p.ParentId.HasValue && publicationIds.Contains(p.ParentId.Value),
                includeProperties: nameof(Publication.Author))
            .AsNoTracking()
            .ToListAsync();

        // Map comments to a lookup for quick access
        var allCommentsLookup = relatedComments
            .Select(mapper.Map<CommentDto>)
            .ToLookup(c => c.ParentId);

        // Assign likes and comments to the resulting list
        foreach (var dto in dtos)
        {
            dto.Likes = allLikesLookup.Contains(dto.Id)
                ? allLikesLookup[dto.Id].ToList()
                : new List<LikeDto>();

            dto.Comments = allCommentsLookup.Contains(dto.Id)
                ? allCommentsLookup[dto.Id].ToList()
                : new List<CommentDto>();
        }

        return dtos;
    }
}


