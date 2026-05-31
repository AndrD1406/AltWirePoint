using AltWirePoint.BusinessLogic.Models.Profile;

namespace AltWirePoint.BusinessLogic.Services.Interfaces;

public interface IFollowService
{
    Task<FollowDto> ToggleFollow(Guid followerId, Guid followedId);
    Task<bool> IsFollowing(Guid followerId, Guid followedId);
    Task<FollowStatsDto> GetFollowStats(Guid userId);
    Task<IEnumerable<ProfileDto>> GetFollowers(Guid userId, int skip, int take);
    Task<IEnumerable<ProfileDto>> GetFollowing(Guid userId, int skip, int take);
}
