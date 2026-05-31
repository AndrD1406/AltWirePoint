namespace AltWirePoint.BusinessLogic.Models.Profile;

public class FollowDto
{
    public Guid FollowerId { get; set; }
    public Guid FollowedId { get; set; }
    public bool IsFollowing { get; set; }
}

public class FollowStatsDto
{
    public int FollowerCount { get; set; }
    public int FollowingCount { get; set; }
}
