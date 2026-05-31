using AltWirePoint.BusinessLogic.Models.Profile;
using AltWirePoint.BusinessLogic.Services.Interfaces;
using AltWirePoint.DataAccess.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AltWirePoint.WebApi.Controllers;

[Authorize]
[Route("api/[controller]/[action]")]
[ApiController]
public class FollowController : ControllerBase
{
    private readonly IFollowService followService;
    private readonly UserManager<ApplicationUser> userManager;

    public FollowController(IFollowService followService, UserManager<ApplicationUser> userManager)
    {
        this.followService = followService;
        this.userManager = userManager;
    }

    [HttpPost("{userId}")]
    [ProducesResponseType(typeof(FollowDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Toggle(Guid userId)
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (currentUserId == null) return Unauthorized();

        var followerId = Guid.Parse(currentUserId);

        if (followerId == userId)
            return BadRequest("You cannot follow yourself.");

        var targetUser = await userManager.FindByIdAsync(userId.ToString());
        if (targetUser == null)
            return NotFound("User not found.");

        var result = await followService.ToggleFollow(followerId, userId);
        return Ok(result);
    }

    [HttpGet("{userId}")]
    [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    public async Task<IActionResult> IsFollowing(Guid userId)
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (currentUserId == null) return Unauthorized();

        var followerId = Guid.Parse(currentUserId);
        var result = await followService.IsFollowing(followerId, userId);
        return Ok(result);
    }

    [HttpGet("{userId}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(FollowStatsDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> Stats(Guid userId)
    {
        var result = await followService.GetFollowStats(userId);
        return Ok(result);
    }

    [HttpGet("{userId}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(IEnumerable<ProfileDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Followers(Guid userId, [FromQuery] int skip = 0, [FromQuery] int take = 20)
    {
        var result = await followService.GetFollowers(userId, skip, take);
        return Ok(result);
    }

    [HttpGet("{userId}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(IEnumerable<ProfileDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Following(Guid userId, [FromQuery] int skip = 0, [FromQuery] int take = 20)
    {
        var result = await followService.GetFollowing(userId, skip, take);
        return Ok(result);
    }
}
