using AltWirePoint.DataAccess.Identity;

namespace AltWirePoint.BusinessLogic.Models.Profile;

public class ProfileDto
{
    public Guid UserId { get; set; }
    public string Name { get; set; } = default!;
    public string? ProfilePictureUrl { get; set; }
    public int? PublicationsCount { get; set; }
}

public static class ProfileDtoExtensions
{
    public static ProfileDto ToProfileDto(this ApplicationUser user)
    {
        return new ProfileDto
        {
            UserId = user.Id,
            Name = user.Name ?? string.Empty,
            ProfilePictureUrl = user.ProfilePicture?.Url
        };
    }
}