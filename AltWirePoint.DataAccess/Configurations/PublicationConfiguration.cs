using AltWirePoint.DataAccess.Models;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AltWirePoint.DataAccess.Configurations;

public class PublicationConfiguration : IEntityTypeConfiguration<Publication>
{
    public void Configure(EntityTypeBuilder<Publication> builder)
    {
        builder.HasOne(p => p.Author)
               .WithMany(u => u.Publications)
               .HasForeignKey(p => p.AuthorId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.Parent)
               .WithMany(p => p.Comments)
               .HasForeignKey(p => p.ParentId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
