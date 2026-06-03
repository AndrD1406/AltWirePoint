using AltWirePoint.BusinessLogic.Models.Moderation;

namespace AltWirePoint.BusinessLogic.Services.Interfaces;

public interface IModerationService
{
    Task BanUserAsync(BanRequest request, Guid adminUserId);
    Task UnbanUserAsync(Guid targetUserId, Guid adminUserId);
    Task<BanStatusDto> GetBanStatusAsync(Guid userId);
}
