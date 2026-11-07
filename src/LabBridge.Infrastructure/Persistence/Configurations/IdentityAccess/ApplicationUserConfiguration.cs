using LabBridge.Domain.Common.ValueObjects;
using LabBridge.Domain.Features.OrganizationManagement.Entities;
using LabBridge.Infrastructure.Identity;
using LabBridge.Infrastructure.Persistence.Configurations.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace LabBridge.Infrastructure.Persistence.Configurations.IdentityAccess;

/// <summary>
/// Identity user. OrganizationId is nullable (null = platform user), therefore the generic tenant owner helper is not used.
/// Never hard-deleted (BR-IAM-010): all relationships are RESTRICT.
/// This configuration runs after IdentityDbContext's default model, so it overrides Identity's defaults.
/// </summary>
public sealed class ApplicationUserConfiguration : IEntityTypeConfiguration<ApplicationUser>
{
    public void Configure(EntityTypeBuilder<ApplicationUser> builder)
    {
        builder.ToTable("users");

        // Keys are assigned by the domain (ApplicationUser.Invite); the database never generates them.
        builder.Property(x => x.Id).ValueGeneratedNever();

        // CHANGE: Identity defaults to 256 characters for user name and e-mail, but EmailAddress allows 320. Longer
        // (valid) addresses would otherwise be rejected by the database.
        builder.Property(x => x.UserName).HasMaxLength(EmailAddress.MaxLength).IsRequired();
        builder.Property(x => x.NormalizedUserName).HasMaxLength(EmailAddress.MaxLength).IsRequired();
        builder.Property(x => x.Email).HasMaxLength(EmailAddress.MaxLength).IsRequired();
        builder.Property(x => x.NormalizedEmail).HasMaxLength(EmailAddress.MaxLength).IsRequired();

        builder.Property(x => x.DisplayName).HasMaxLength(200).IsRequired();
        // CHANGE: removed HasDefaultValue(UserStatus.Invited). Invited is the CLR default (0), which EF treats as
        // "unset", so the default was redundant and a known source of surprises.
        builder.Property(x => x.Status).HasEnumAsString(20);
        builder.Property(x => x.MustChangePassword).IsRequired();
        builder.Property(x => x.CreatedAt).IsRequired();
        builder.Property(x => x.UpdatedAt).IsRequired();

        builder
            .HasOne<Organization>()
            .WithMany()
            .HasForeignKey(x => x.OrganizationId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);

        // CHANGE: platform-wide e-mail uniqueness (BR-IAM-016) is enforced by the database. Identity's default
        // EmailIndex is NOT unique (uniqueness is only checked in UserManager when RequireUniqueEmail is set, which
        // is racy). The same index is reused and renamed to the project convention.
        builder
            .HasIndex(x => x.NormalizedEmail)
            .IsUnique()
            .HasDatabaseName("ux_users_normalized_email");
        builder
            .HasIndex(x => x.NormalizedUserName)
            .IsUnique()
            .HasDatabaseName("ux_users_normalized_user_name");
        builder.HasIndex(x => new { x.OrganizationId, x.Status });
        builder.HasIndex(x => new { x.OrganizationId, x.DisplayName });
        builder.HasIndex(x => new { x.OrganizationId, x.CreatedAt });

        builder
            .HasMany(x => x.RoleAssignments)
            .WithOne()
            .HasForeignKey(a => a.UserId)
            .OnDelete(DeleteBehavior.Restrict);
        builder
            .Navigation(x => x.RoleAssignments)
            .HasField("_roleAssignments")
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder
            .HasMany(x => x.Invitations)
            .WithOne()
            .HasForeignKey(i => i.UserId)
            .OnDelete(DeleteBehavior.Restrict);
        builder
            .Navigation(x => x.Invitations)
            .HasField("_invitations")
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Ignore(x => x.IsPlatformUser);

        // IMPROVEMENT: two concurrency tokens are active on purpose. xmin backs the API ETag (FRS), while Identity's
        // ConcurrencyStamp (kept by default) is what UserManager.UpdateAsync checks. Both are validated on update.
        builder.UseXminConcurrency();
        builder.UseSnakeCaseColumns();
    }
}