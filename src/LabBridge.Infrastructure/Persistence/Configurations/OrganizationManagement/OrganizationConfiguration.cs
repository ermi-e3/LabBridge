using LabBridge.Domain.Features.OrganizationManagement;
using LabBridge.Domain.Features.OrganizationManagement.Entities;
using LabBridge.Infrastructure.Persistence.Configurations.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LabBridge.Infrastructure.Persistence.Configurations.OrganizationManagement;

public sealed class OrganizationConfiguration : IEntityTypeConfiguration<Organization>
{
    public void Configure(EntityTypeBuilder<Organization> builder)
    {
        builder.ToTable(
            "organizations", "organization_management",
            t =>
            {
                t.HasCheckConstraint(
                    "ck_organizations_suspension_mode",
                    "(status = 'Suspended' AND suspension_mode IS NOT NULL) OR (status <> 'Suspended' AND suspension_mode IS NULL)"
                );
                t.HasCheckConstraint(
                    "ck_organizations_status_reason",
                    "status NOT IN ('Suspended','Deactivated') OR status_reason IS NOT NULL"
                );
                // Case-sensitive now that the column is varchar, so uppercase codes are rejected exactly as the domain rejects them.
                t.HasCheckConstraint(
                    "ck_organizations_code_format",
                    "code ~ '^[a-z0-9]+(-[a-z0-9]+)*$' AND char_length(code) BETWEEN 3 AND 40"
                );
                // An organization that was activated must have its initial administrator (Activate enforces it).
                t.HasCheckConstraint(
                    "ck_organizations_initial_admin",
                    "status NOT IN ('Active','Suspended') OR initial_admin_provisioned"
                );
                t.HasCheckConstraint("ck_organizations_status_version", "status_version >= 1");
            }
        );

        builder.HasDomainAssignedKey();
        //builder.Property(x => x.Code).HasCaseInsensitiveText(40).IsRequired();
        // CHANGE: plain varchar instead of citext. The domain forces lowercase, citext adds nothing here, and on a citext column the regex check
        // above would match case-insensitively. If the column is already citext in a migration, this change needs a column type migration.
        builder.Property(x => x.Code).HasMaxLength(40).IsRequired();
        builder.Property(x => x.DisplayName).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Status).HasEnumAsString(20);
        builder.Property(x => x.SuspensionMode).HasEnumAsString(20);
        builder.Property(x => x.StatusReason).HasMaxLength(500);
        builder.Property(x => x.StatusChangedAt).IsRequired();
        builder.Property(x => x.StatusVersion).IsRequired().HasDefaultValue(1);
        builder
            .Property(x => x.DefaultTimeZone)
            .HasMaxLength(64)
            .IsRequired()
            .HasDefaultValue("UTC");
        builder.Property(x => x.InitialAdminProvisioned).IsRequired().HasDefaultValue(false);
        builder.Property(x => x.CreatedAt).IsRequired();
        builder.Property(x => x.UpdatedAt).IsRequired();

        builder.HasIndex(x => x.Code).IsUnique().HasDatabaseName("ux_organizations_code");
        builder.HasIndex(x => x.Status);
        builder.HasIndex(x => x.CreatedAt);
        builder.HasIndex(x => x.DisplayName);

        builder
            .HasMany(x => x.StatusHistory)
            .WithOne()
            .HasForeignKey(h => h.OrganizationId)
            .OnDelete(DeleteBehavior.Restrict);
        builder
            .Navigation(x => x.StatusHistory)
            .HasField("_statusHistory")
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Ignore(x => x.OperatingPolicy);
        builder.IgnoreDomainEvents();
        builder.UseXminConcurrency();
        builder.UseSnakeCaseColumns();
    }
}
