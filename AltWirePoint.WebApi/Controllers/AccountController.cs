using AltWirePoint.BusinessLogic.Models.Identity;
using AltWirePoint.BusinessLogic.Models.Profile;
using AltWirePoint.BusinessLogic.Services;
using AltWirePoint.BusinessLogic.Services.Interfaces;
using AltWirePoint.DataAccess;
using AltWirePoint.DataAccess.Identity;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace AltWirePoint.WebApi.Controllers;

[AllowAnonymous]
[Route("api/[controller]/[action]")]
[ApiController]
public class AccountController : ControllerBase
{
    private readonly UserManager<ApplicationUser> userManager;
    private readonly SignInManager<ApplicationUser> signInManager;
    private readonly IJwtService jwtService;
    private readonly ICloudStoredFileService cloudStoredFileService;
    private readonly AltWirePointDbContext dbContext;
    private readonly IMapper mapper;

    public AccountController(UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager, IJwtService jwtService,
        ICloudStoredFileService cloudStoredFileService, AltWirePointDbContext dbContext,
        IMapper mapp)
    {
        this.userManager = userManager;
        this.signInManager = signInManager;
        this.jwtService = jwtService;
        this.cloudStoredFileService = cloudStoredFileService;
        this.dbContext = dbContext;
        mapper = mapp;
    }

    [HttpPost]
    [ProducesResponseType(typeof(AuthenticationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Register(RegisterRequest registerRequest)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var user = mapper.Map<ApplicationUser>(registerRequest);

        var result = await userManager.CreateAsync(user, registerRequest.Password);

        if (result.Succeeded)
        {
            var authenticationResponse = await jwtService.CreateJwtToken(user);

            user.RefreshToken = authenticationResponse.RefreshToken;
            user.RefreshTokenExpirationDateTime = authenticationResponse.RefreshTokenExpirationDateTime;
            await userManager.UpdateAsync(user);

            return Ok(authenticationResponse);
        }

        return BadRequest(result.Errors);
    }

    [HttpGet]
    public async Task<IActionResult> IsEmailAlreadyRegistered(string email)
    {
        ApplicationUser? user = await userManager.FindByEmailAsync(email);

        if (user == null)
        {
            return Ok(true);
        }
        return Ok(false);
    }

    [HttpPost]
    [ProducesResponseType(typeof(AuthenticationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Login(LoginRequest loginRequest)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var user = await userManager.FindByEmailAsync(loginRequest.Email);
        if (user == null) return BadRequest("Invalid email or password");

        var result = await signInManager.CheckPasswordSignInAsync(
            user,
            loginRequest.Password,
            lockoutOnFailure: true
        );

        if (result.Succeeded)
        {
            var authenticationResponse = await jwtService.CreateJwtToken(user);

            user.RefreshToken = authenticationResponse.RefreshToken;
            user.RefreshTokenExpirationDateTime = authenticationResponse.RefreshTokenExpirationDateTime;
            await userManager.UpdateAsync(user);

            return Ok(authenticationResponse);
        }

        if (result.IsLockedOut)
        {
            return BadRequest("Account is locked out. Try again later.");
        }

        return BadRequest("Invalid email or password");
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> Logout()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId != null)
        {
            var user = await userManager.FindByIdAsync(userId);
            if (user != null)
            {
                user.RefreshToken = null;
                user.RefreshTokenExpirationDateTime = DateTime.MinValue;
                await userManager.UpdateAsync(user);
            }
        }

        return NoContent();
    }

    [HttpPost]
    [ProducesResponseType(typeof(AuthenticationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Refresh(TokenModel tokenModel)
    {
        if (tokenModel == null)
        {
            return BadRequest("Invalid client request");
        }

        string? token = tokenModel.Token;
        string? refreshToken = tokenModel.RefreshToken;

        ClaimsPrincipal? principal = jwtService.GetPrincipalFromJwtToken(token);
        if (principal == null)
        {
            return BadRequest("Invalid access token");
        }

        string? email = principal.FindFirstValue(ClaimTypes.Email);

        ApplicationUser? user = await userManager.FindByEmailAsync(email);

        if (user == null || user.RefreshToken != refreshToken || user.RefreshTokenExpirationDateTime <= DateTime.UtcNow)
        {
            return BadRequest("Invalid refresh token");
        }

        AuthenticationResponse authenticationResponse = await jwtService.CreateJwtToken(user);

        user.RefreshToken = authenticationResponse.RefreshToken;
        user.RefreshTokenExpirationDateTime = authenticationResponse.RefreshTokenExpirationDateTime;

        await userManager.UpdateAsync(user);

        return Ok(authenticationResponse);
    }

    [HttpPost]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        if (request.NewPassword != request.ConfirmPassword)
            return BadRequest("New password and confirmation do not match.");

        var userIdClaim = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
        if (userIdClaim == null) return Unauthorized();
        var user = await userManager.FindByIdAsync(userIdClaim);
        if (user == null) return NotFound();

        var result = await userManager.ChangePasswordAsync(
            user,
            request.CurrentPassword,
            request.NewPassword
        );

        if (!result.Succeeded)
            return BadRequest(result.Errors.Select(e => e.Description));

        return NoContent();
    }

    [HttpPut]
    [Authorize]
    [ProducesResponseType(typeof(ProfileDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> EditProfile([FromForm] ProfileEditRequest request, IFormFile? profilePicture)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (userId == null) return Unauthorized();

        var user = await userManager.FindByIdAsync(userId);
        if (user == null) return NotFound();

        if (request != null && !string.IsNullOrWhiteSpace(request.Name))
            user.Name = request.Name;

        if (profilePicture != null && profilePicture.Length > 0)
        {
            if (!profilePicture.ContentType.StartsWith("image"))
                return BadRequest("Only image files are allowed for profile pictures.");

            var existingPfp = await dbContext.CloudStoredFiles
                .FirstOrDefaultAsync(f => f.ApplicationUserId == user.Id);

            if (existingPfp != null)
            {
                await cloudStoredFileService.DeleteFileAsync(existingPfp.Url);
                dbContext.CloudStoredFiles.Remove(existingPfp);
            }

            var storedFile = await cloudStoredFileService.UploadFileAsync(
                profilePicture.OpenReadStream(),
                profilePicture.FileName,
                profilePicture.ContentType,
                CloudStoredFileService.ContainerNames.ProfilePictures);

            storedFile.ApplicationUserId = user.Id;
            dbContext.CloudStoredFiles.Add(storedFile);

            await dbContext.SaveChangesAsync();
        }

        var result = await userManager.UpdateAsync(user);
        if (!result.Succeeded)
            return BadRequest(result.Errors.Select(e => e.Description));

        var profileDto = user.ToProfileDto();
        return Ok(profileDto);
    }

    [HttpGet("{userId}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetProfilePicture(Guid userId)
    {
        var file = await dbContext.CloudStoredFiles
            .FirstOrDefaultAsync(f => f.ApplicationUserId == userId);

        if (file == null)
            return NotFound("No profile picture found for this user.");

        return Ok(new { url = file.Url });
    }

    [HttpGet("{id}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ProfileDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetUserById(Guid id)
    {
        var user = await userManager.FindByIdAsync(id.ToString());
        if (user == null)
            return NotFound();

        var dto = user.ToProfileDto();

        return Ok(dto);
    }
}
