using AltWirePoint.DataAccess.Models;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AltWirePoint.DataAccess.Configurations;

public class LikeConfiguration : IEntityTypeConfiguration<Like>
{
    public void Configure(EntityTypeBuilder<Like> builder)
    {
        builder.HasOne(l => l.Author)
               .WithMany(u => u.Likes)
               .HasForeignKey(l => l.AuthorId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(l => l.Publication)
               .WithMany(p => p.Likes)
               .HasForeignKey(l => l.PublicationId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}