using AltWirePoint.DataAccess.Models;
using Microsoft.AspNetCore.Identity;

namespace AltWirePoint.DataAccess.Identity;

public class ApplicationUser : IdentityUser<Guid>
{
    public string? Name { get; set; }

    public string Role { get; set; }

    public string? Description { get; set; }

    public string? RefreshToken { get; set; }

    public DateTime RefreshTokenExpirationDateTime { get; set; }

    public virtual List<Publication>? Publications { get; set; }

    public virtual List<Like>? Likes { get; set; }

    public virtual CloudStoredFile? ProfilePicture { get; set; }
}
