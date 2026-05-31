using AltWirePoint.BusinessLogic.Models.Profile;
using AltWirePoint.BusinessLogic.Services.Interfaces;
using AltWirePoint.DataAccess.Enums;
using AltWirePoint.DataAccess.Models;
using AltWirePoint.DataAccess.Repository.Base;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace AltWirePoint.BusinessLogic.Services;

public class FollowService : IFollowService
{
    private readonly IEntityRepository<Guid, UserFollow> followRepository;

    public FollowService(IEntityRepository<Guid, UserFollow> followRepository)
    {
        this.followRepository = followRepository;
    }

    public async Task<FollowDto> ToggleFollow(Guid followerId, Guid followedId)
    {
        if (followerId == followedId)
            throw new InvalidOperationException("A user cannot follow themselves.");

        var existing = await followRepository
            .GetByFilter(f => f.FollowerId == followerId && f.FollowedId == followedId)
            .FirstOrDefaultAsync();

        if (existing != null)
        {
            await followRepository.Delete(existing);
            return new FollowDto
            {
                FollowerId = followerId,
                FollowedId = followedId,
                IsFollowing = false
            };
        }

        var follow = new UserFollow
        {
            FollowerId = followerId,
            FollowedId = followedId,
            FollowedAt = DateTime.UtcNow
        };

        await followRepository.Create(follow);

        return new FollowDto
        {
            FollowerId = followerId,
            FollowedId = followedId,
            IsFollowing = true
        };
    }

    public async Task<bool> IsFollowing(Guid followerId, Guid followedId)
    {
        return await followRepository
            .Any(f => f.FollowerId == followerId && f.FollowedId == followedId);
    }

    public async Task<FollowStatsDto> GetFollowStats(Guid userId)
    {
        var followerCount = await followRepository.Count(f => f.FollowedId == userId);
        var followingCount = await followRepository.Count(f => f.FollowerId == userId);

        return new FollowStatsDto
        {
            FollowerCount = followerCount,
            FollowingCount = followingCount
        };
    }

    public async Task<IEnumerable<ProfileDto>> GetFollowers(Guid userId, int skip, int take)
    {
        var follows = await followRepository
            .Get(
                skip: skip,
                take: take,
                includeProperties: "Follower.ProfilePicture",
                whereExpression: f => f.FollowedId == userId,
                orderBy: new List<(Expression<Func<UserFollow, object>>, SortDirection)>
                {
                    (f => f.FollowedAt, SortDirection.Descending)
                }
            )
            .AsNoTracking()
            .ToListAsync();

        return follows.Select(f => f.Follower!.ToProfileDto());
    }

    public async Task<IEnumerable<ProfileDto>> GetFollowing(Guid userId, int skip, int take)
    {
        var follows = await followRepository
            .Get(
                skip: skip,
                take: take,
                includeProperties: "Followed.ProfilePicture",
                whereExpression: f => f.FollowerId == userId,
                orderBy: new List<(Expression<Func<UserFollow, object>>, SortDirection)>
                {
                    (f => f.FollowedAt, SortDirection.Descending)
                }
            )
            .AsNoTracking()
            .ToListAsync();

        return follows.Select(f => f.Followed!.ToProfileDto());
    }
}
