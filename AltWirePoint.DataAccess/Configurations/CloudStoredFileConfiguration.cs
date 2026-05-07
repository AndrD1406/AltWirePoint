using AltWirePoint.DataAccess.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AltWirePoint.DataAccess.Configurations;

public class CloudStoredFileConfiguration : IEntityTypeConfiguration<CloudStoredFile>
{
    public void Configure(EntityTypeBuilder<CloudStoredFile> builder)
    {
        builder.HasKey(f => f.Id);
        builder.Property(f => f.Url).IsRequired().HasMaxLength(2048);
        builder.Property(f => f.FileType).IsRequired();
        builder.Property(f => f.FileSize).IsRequired();

        builder.HasOne(f => f.Publication)
               .WithMany(p => p.CloudStoredFiles)
               .HasForeignKey(f => f.PublicationId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(f => f.ApplicationUser)
               .WithOne(u => u.ProfilePicture)
               .HasForeignKey<CloudStoredFile>(f => f.ApplicationUserId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
