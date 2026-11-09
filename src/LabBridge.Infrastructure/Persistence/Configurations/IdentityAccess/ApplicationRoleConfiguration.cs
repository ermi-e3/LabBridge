using LabBridge.Infrastructure.Identity.Entities;
using LabBridge.Infrastructure.Persistence.Configurations.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace LabBridge.Infrastructure.Persistence.Configurations.IdentityAccess;


/// <summary>Platform reference data, changed only by released migrations (BR-IAM-011). Permission set is role-owned.</summary>
public sealed class ApplicationRoleConfiguration : IEntityTypeConfiguration<ApplicationRole>
{
    public void Configure(EntityTypeBuilder<ApplicationRole> builder)
    {
        builder.ToTable("roles", "identity");

        builder.Property(x => x.Id).ValueGeneratedNever();

        // Identity's Name holds the stable role code (max 50, as the former Code column).
        builder.Property(x => x.Name).HasMaxLength(50).IsRequired();
        builder.Property(x => x.NormalizedName).HasMaxLength(50).IsRequired();
        builder.Property(x => x.DisplayName).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(500);
        builder.Property(x => x.Scope).HasEnumAsString(20);

        // CHANGE: removed HasDefaultValue(true) on IsSystem. For a bool, EF treats the CLR default (false) as "unset" and
        // would silently persist every IsSystem = false role as true.
        builder.Property(x => x.IsSystem).IsRequired();
        builder.Property(x => x.IsAdditive).IsRequired();
        builder.Property(x => x.CreatedAt).IsRequired();

        // Replaces the former unique index on Code. This reuses Identity's RoleNameIndex and renames it.
        builder
            .HasIndex(x => x.NormalizedName)
            .IsUnique()
            .HasDatabaseName("ux_roles_normalized_name");

        builder
            .HasMany(x => x.Permissions)
            .WithOne()
            .HasForeignKey(p => p.RoleId)
            .OnDelete(DeleteBehavior.Restrict);
        builder
            .Navigation(x => x.Permissions)
            .HasField("_permissions")
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        // Code is an alias of Name, not a column.
        builder.Ignore(x => x.Code);

        builder.UseSnakeCaseColumns();
    }
}