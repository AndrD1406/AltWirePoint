using AltWirePoint.BusinessLogic.Models.Moderation;
using AltWirePoint.BusinessLogic.Services.Interfaces;
using AltWirePoint.Common.PermissionManagement;
using AltWirePoint.Common.PermissionModule.PolicyClasses;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AltWirePoint.WebApi.Controllers;

[Route("api/[controller]/[action]")]
[ApiController]
public class ModerationController : ControllerBase
{
    private readonly IModerationService moderationService;

    public ModerationController(IModerationService moderationService)
    {
        this.moderationService = moderationService;
    }

    [HttpPost]
    [HasPermission(Permissions.BanUsers)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> BanUser([FromBody] BanRequest request)
    {
        var adminIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (adminIdClaim == null || !Guid.TryParse(adminIdClaim, out var adminId))
            return Unauthorized();

        try
        {
            await moderationService.BanUserAsync(request, adminId);
            return NoContent();
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("{userId}")]
    [HasPermission(Permissions.BanUsers)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UnbanUser(Guid userId)
    {
        var adminIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (adminIdClaim == null || !Guid.TryParse(adminIdClaim, out var adminId))
            return Unauthorized();

        try
        {
            await moderationService.UnbanUserAsync(userId, adminId);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }
}
