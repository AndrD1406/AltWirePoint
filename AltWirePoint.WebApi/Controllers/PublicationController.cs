using AltWirePoint.BusinessLogic.Models;
using AltWirePoint.BusinessLogic.Models.Profile;
using AltWirePoint.BusinessLogic.Models.Publication;
using AltWirePoint.BusinessLogic.Services.Interfaces;
using AltWirePoint.DataAccess.Identity;
using AltWirePoint.DataAccess.Models;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AltWirePoint.WebApi.Controllers;

[Route("api/[controller]/[action]")]
[ApiController]
public class PublicationController : ControllerBase
{
    private readonly IPublicationService publicationService;
    private readonly UserManager<ApplicationUser> userManager;
    private readonly IMapper mapper;

    public PublicationController(IPublicationService publicationService, IMapper mapper, UserManager<ApplicationUser> userManager)
    {
        this.publicationService = publicationService;
        this.mapper = mapper;
        this.userManager = userManager;
    }

    [HttpPost]
    [Authorize]
    [ProducesResponseType(typeof(Publication), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromForm] PublicationCreateRequest request, [FromForm] List<IFormFile> files)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null)
            return Unauthorized();

        var fileDtos = new List<FileUploadDto>();
        if (files != null)
        {
            foreach (var file in files)
            {
                fileDtos.Add(new FileUploadDto
                {
                    Content = file.OpenReadStream(),
                    FileName = file.FileName,
                    ContentType = file.ContentType,
                    Length = file.Length
                });
            }
        }

        var publication = await publicationService.Create(request, Guid.Parse(userId), fileDtos);

        return CreatedAtAction(
            nameof(GetById),
            new { id = publication.Id },
            publication
        );
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(Publication), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var publication = await publicationService.GetById(id);
        if (publication == null)
            return NotFound();

        return Ok(publication);
    }

    [HttpPut]
    [ProducesResponseType(typeof(LikeDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> Like(Guid id)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userIdClaim == null || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized();
        }

        var likeDto = await publicationService.SetLike(id, userId);

        return Ok(likeDto);
    }

    [HttpGet]
    [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPulicationCountByAuthor(Guid id)
    {
        var publicationsCount = await publicationService.GetPublicationCountByAuthor(id);
        return Ok(publicationsCount);
    }

    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ProfileDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetUserById(Guid id)
    {
        var user = await userManager.FindByIdAsync(id.ToString());
        if (user == null)
            return NotFound();

        var dto = mapper.Map<ProfileDto>(user);

        return Ok(dto);
    }

    [HttpPost]
    [Authorize]
    [ProducesResponseType(typeof(CommentDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Comment([FromForm] CommentCreateRequest request, [FromForm] List<IFormFile> files)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null || !Guid.TryParse(userId, out var currentUserId))
            return Unauthorized();

        if (request.AuthorId != currentUserId)
            return Forbid();

        var fileDtos = new List<FileUploadDto>();
        if (files != null)
        {
            foreach (var file in files)
            {
                fileDtos.Add(new FileUploadDto
                {
                    Content = file.OpenReadStream(),
                    FileName = file.FileName,
                    ContentType = file.ContentType,
                    Length = file.Length
                });
            }
        }

        var comment = await publicationService.AddComment(request, fileDtos);

        return CreatedAtAction(
            nameof(GetById),
            new { id = comment.Id },
            comment
        );
    }

    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(IEnumerable<PublicationDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Get(
    [FromQuery] int skip = 0,
    [FromQuery] int take = 10)
    {
        var page = await publicationService.Get(skip, take);
        return Ok(page);
    }

    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(IEnumerable<PublicationDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByUserIdPaged(
        [FromQuery] Guid id,
        [FromQuery] int skip = 0,
        [FromQuery] int take = 10)
    {
        var page = await publicationService.GetPublicationsByAuthorPaged(id, skip, take);
        return Ok(page);
    }

    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(IEnumerable<PublicationDto>), StatusCodes.Status200OK)]
    public Task<IEnumerable<PublicationDto>> Search(string query, int skipCount = 0, int maxResultCount = 10)
    => publicationService.SearchAsync(query, skipCount, maxResultCount);

    [HttpPut]
    [Authorize]
    [ProducesResponseType(typeof(IEnumerable<PublicationDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PublicationDto>> Update(Guid id, [FromBody] PublicationCreateRequest dto)
    {
        var updated = await publicationService.Update(id, dto);
        return Ok(updated);
    }

    [HttpDelete]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Delete(Guid id)
    {
        await publicationService.Delete(id);
        return NoContent();
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<CommentDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<CommentDto>>> GetCommentsForPublication(Guid id)
    {
        var comments = await publicationService.GetCommentsForPublication(id);
        return Ok(comments);
    }
}
