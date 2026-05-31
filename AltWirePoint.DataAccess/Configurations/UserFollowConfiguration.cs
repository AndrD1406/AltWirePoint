using AltWirePoint.DataAccess.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AltWirePoint.DataAccess.Configurations;

public class UserFollowConfiguration : IEntityTypeConfiguration<UserFollow>
{
    public void Configure(EntityTypeBuilder<UserFollow> builder)
    {
        builder.HasOne(f => f.Follower)
               .WithMany(u => u.Following)
               .HasForeignKey(f => f.FollowerId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(f => f.Followed)
               .WithMany(u => u.Followers)
               .HasForeignKey(f => f.FollowedId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(f => new { f.FollowerId, f.FollowedId })
               .IsUnique();

        builder.ToTable(t =>
            t.HasCheckConstraint("CK_UserFollow_NoSelfFollow", "[FollowerId] <> [FollowedId]"));
    }
}
