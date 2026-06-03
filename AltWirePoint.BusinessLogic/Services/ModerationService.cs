using AltWirePoint.BusinessLogic.Models.Moderation;
using AltWirePoint.BusinessLogic.Services.Interfaces;
using AltWirePoint.DataAccess;
using AltWirePoint.DataAccess.Identity;
using AltWirePoint.DataAccess.Models;
using AltWirePoint.DataAccess.Repository.Base;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AltWirePoint.BusinessLogic.Services;

public class ModerationService : IModerationService
{
    private readonly IEntityRepository<long, UserBan> userBanRepository;
    private readonly IEntityRepository<Guid, Publication> publicationRepository;
    private readonly UserManager<ApplicationUser> userManager;

    public ModerationService(
        IEntityRepository<long, UserBan> userBanRepository, 
        IEntityRepository<Guid, Publication> publicationRepository,
        UserManager<ApplicationUser> userManager)
    {
        this.userBanRepository = userBanRepository;
        this.publicationRepository = publicationRepository;
        this.userManager = userManager;
    }

    public async Task BanUserAsync(BanRequest request, Guid adminUserId)
    {
        var targetUser = await userManager.FindByIdAsync(request.UserId.ToString());
        if (targetUser == null)
            throw new ArgumentException("Target user not found");

        if (request.UserId == adminUserId)
            throw new InvalidOperationException("You cannot ban yourself");

        // Deactivate any existing active bans for this user to avoid duplicates
        var existingBans = await userBanRepository
            .GetByFilter(b => b.UserId == request.UserId && b.IsActive)
            .ToListAsync();

        foreach (var ban in existingBans)
        {
            ban.IsActive = false;
        }

        var newBan = new UserBan
        {
            UserId = request.UserId,
            BannedByUserId = adminUserId,
            BannedAt = DateTime.UtcNow,
            BannedUntil = request.BannedUntil,
            Reason = request.Reason ?? string.Empty,
            IsActive = true
        };

        await userBanRepository.Create(newBan);

        // Invalidate refresh token to force re-authentication (which will be blocked by middleware)
        targetUser.RefreshToken = null;
        targetUser.RefreshTokenExpirationDateTime = DateTime.MinValue;
        await userManager.UpdateAsync(targetUser);

        // Hide all publications by the banned user
        var userPublications = await publicationRepository
            .GetByFilter(p => p.AuthorId == request.UserId && !p.IsHidden)
            .ToListAsync();
            
        foreach (var publication in userPublications)
        {
            publication.IsHidden = true;
        }

        await userBanRepository.SaveChangesAsync();
    }

    public async Task UnbanUserAsync(Guid targetUserId, Guid adminUserId)
    {
        var activeBans = await userBanRepository
            .GetByFilter(b => b.UserId == targetUserId && b.IsActive)
            .ToListAsync();

        if (!activeBans.Any())
            throw new InvalidOperationException("User is not currently banned");

        foreach (var ban in activeBans)
        {
            ban.IsActive = false;
        }

        // Restore all publications by the unbanned user
        var userPublications = await publicationRepository
            .GetByFilter(p => p.AuthorId == targetUserId && p.IsHidden)
            .ToListAsync();
            
        foreach (var publication in userPublications)
        {
            publication.IsHidden = false;
        }

        await userBanRepository.SaveChangesAsync();
    }

    public async Task<BanStatusDto> GetBanStatusAsync(Guid userId)
    {
        var activeBan = await userBanRepository
            .GetByFilter(b => b.UserId == userId && b.IsActive && (b.BannedUntil == null || b.BannedUntil > DateTime.UtcNow))
            .OrderByDescending(b => b.BannedAt)
            .FirstOrDefaultAsync();

        if (activeBan == null)
        {
            return new BanStatusDto { IsBanned = false };
        }

        return new BanStatusDto
        {
            IsBanned = true,
            Reason = activeBan.Reason,
            BannedAt = activeBan.BannedAt,
            BannedUntil = activeBan.BannedUntil,
            BannedByUserId = activeBan.BannedByUserId
        };
    }
}
