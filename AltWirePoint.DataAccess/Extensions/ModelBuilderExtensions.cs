using AltWirePoint.DataAccess.Enums;
using AltWirePoint.DataAccess.Identity;
using AltWirePoint.DataAccess.Models;
using Microsoft.EntityFrameworkCore;

namespace AltWirePoint.DataAccess.Extensions;

public static class ModelBuilderExtensions
{
    public static void Seed(this ModelBuilder builder)
    {
        builder.Entity<PermissionsForRole>().HasData(
            new PermissionsForRole
            {
                Id = 1,
                RoleName = Role.User.ToString(),
                PackedPermissions = Common.PermissionManagement.PermissionSeeder.SeedPermissions(Role.User.ToString())!,
                Description = "Default permissions for user role"
            },
            new PermissionsForRole
            {
                Id = 2,
                RoleName = Role.Admin.ToString(),
                PackedPermissions = Common.PermissionManagement.PermissionSeeder.SeedPermissions(Role.Admin.ToString())!,
                Description = "Default permissions for admin role"
            }
        );
    }
}
