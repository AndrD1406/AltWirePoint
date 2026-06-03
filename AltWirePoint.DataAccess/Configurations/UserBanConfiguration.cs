using AltWirePoint.DataAccess.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AltWirePoint.DataAccess.Configurations;

public class UserBanConfiguration : IEntityTypeConfiguration<UserBan>
{
    public void Configure(EntityTypeBuilder<UserBan> builder)
    {
        builder.HasKey(b => b.Id);

        builder.Property(b => b.Reason)
               .HasMaxLength(500);

        // Prevent multiple cascade paths error in SQL Server
        builder.HasOne(b => b.User)
               .WithMany()
               .HasForeignKey(b => b.UserId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(b => b.BannedByUser)
               .WithMany()
               .HasForeignKey(b => b.BannedByUserId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
