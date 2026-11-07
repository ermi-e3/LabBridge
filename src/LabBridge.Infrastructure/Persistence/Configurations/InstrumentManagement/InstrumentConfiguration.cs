using LabBridge.Domain.Features.InstrumentManagement.Entities;
using LabBridge.Domain.Features.InstrumentManagement.Enums;
using LabBridge.Infrastructure.Persistence.Configurations.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LabBridge.Infrastructure.Persistence.Configurations.InstrumentManagement;

/// <summary>Tenant-owned instrument. Verification records are aggregate-owned; systems are referenced through composite tenant FKs (SC-ORG-003).</summary>
public sealed class InstrumentConfiguration : IEntityTypeConfiguration<Instrument>
{
    public void Configure(EntityTypeBuilder<Instrument> builder)
    {
        builder.ToTable(
            "instruments",
            t =>
            {
                t.HasCheckConstraint(
                    "ck_instruments_simulated_no_class",
                    "NOT is_simulated OR compatibility_class IS NULL"
                );
                t.HasCheckConstraint(
                    "ck_instruments_simulated_not_hardware_verified",
                    "NOT is_simulated OR verification_status <> 'HardwareVerified'"
                );
                t.HasCheckConstraint(
                    "ck_instruments_class_basis",
                    "(COALESCE(compatibility_class, '') = 'C1') = (compatibility_basis_profile_version_id IS NOT NULL)"
                );
                t.HasCheckConstraint(
                    "ck_instruments_topology",
                    "(topology IS NULL) = (order_source_system_id IS NULL OR result_destination_system_id IS NULL)"
                );
                t.HasCheckConstraint("ck_instruments_class_rationale",
                    "compatibility_class IS NULL OR compatibility_class = 'C1' OR compatibility_rationale IS NOT NULL");
            }
        );

        builder.HasDomainAssignedKey();
        builder.HasTenantAlternateKey();
        builder.Property(x => x.Name).HasCaseInsensitiveText(200).IsRequired();
        builder.Property(x => x.Manufacturer).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Model).HasMaxLength(100).IsRequired();
        builder.Property(x => x.SerialNumber).HasMaxLength(100);
        builder.Property(x => x.Location).HasMaxLength(200);
        builder.Property(x => x.Notes).HasMaxLength(1000);
        builder.Property(x => x.IsSimulated).IsRequired();
        builder.Property(x => x.CompatibilityClass).HasEnumAsString(2);
        builder.Property(x => x.CompatibilityRationale).HasMaxLength(1000);
        builder
            .Property(x => x.VerificationStatus)
            .HasEnumAsString(20)
            .HasDefaultValue(VerificationStatus.Unverified);
        builder.Property(x => x.Topology).HasEnumAsString(20);
        builder.Property(x => x.Status).HasEnumAsString(10).HasDefaultValue(ActiveStatus.Active);
        builder.Property(x => x.CreatedAt).IsRequired();
        builder.Property(x => x.UpdatedAt).IsRequired();

        builder.HasOrganizationOwner();
        builder
            .HasOne<ExternalSystem>()
            .WithMany()
            .HasForeignKey(x => new { x.OrganizationId, x.OrderSourceSystemId })
            .HasPrincipalKey(s => new { s.OrganizationId, s.Id })
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);
        builder
            .HasOne<ExternalSystem>()
            .WithMany()
            .HasForeignKey(x => new { x.OrganizationId, x.ResultDestinationSystemId })
            .HasPrincipalKey(s => new { s.OrganizationId, s.Id })
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);
        builder
            .HasOne<InterfaceProfileVersion>()
            .WithMany()
            .HasForeignKey(x => x.CompatibilityBasisProfileVersionId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasMany(x => x.Verifications)
            .WithOne()
            .HasForeignKey(v => new { v.OrganizationId, v.InstrumentId })
            .HasPrincipalKey(i => new { i.OrganizationId, i.Id })
            .OnDelete(DeleteBehavior.Restrict);
        builder
            .Navigation(x => x.Verifications)
            .HasField("_verifications")
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasIndex(x => new { x.OrganizationId, x.Name }).IsUnique();
        builder.HasIndex(x => new { x.OrganizationId, x.Status });
        builder.HasIndex(x => new { x.OrganizationId, x.VerificationStatus });

        builder.IgnoreDomainEvents();
        builder.UseXminConcurrency();
        builder.UseSnakeCaseColumns();
    }
}
