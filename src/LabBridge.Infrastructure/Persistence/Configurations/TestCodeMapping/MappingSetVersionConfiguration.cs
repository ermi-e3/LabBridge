using LabBridge.Domain.Features.TestCodeMapping.Entities;
using LabBridge.Domain.Features.TestCodeMapping.Enums;
using LabBridge.Infrastructure.Persistence.Configurations.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LabBridge.Infrastructure.Persistence.Configurations.TestCodeMapping;

/// <summary>One Draft and one Published version per set (partial unique indexes). Direction is a denormalised copy of the set direction (A-MAP-03).</summary>
public sealed class MappingSetVersionConfiguration : IEntityTypeConfiguration<MappingSetVersion>
{
    public void Configure(EntityTypeBuilder<MappingSetVersion> builder)
    {
        builder.ToTable(
                "mapping_set_versions",
                t =>
                {
                    t.HasCheckConstraint(
                        "ck_mapping_set_versions_published",
                        "status NOT IN ('Published','Superseded') OR (change_reason IS NOT NULL AND published_at IS NOT NULL)"
                    );
                    t.HasCheckConstraint("ck_mapping_set_versions_draft", "(status = 'Draft') = (published_at IS NULL)");
                    t.HasCheckConstraint("ck_mapping_set_versions_published_by", "(published_at IS NULL) = (published_by IS NULL)");
                    t.HasCheckConstraint("ck_mapping_set_versions_number", "version_number >= 1");
                }
            );
        builder.HasDomainAssignedKey();
        builder.HasTenantAlternateKey();
        builder.Property(x => x.VersionNumber).IsRequired();
        builder.Property(x => x.Status).HasEnumAsString(12);
        builder
            .Property(x => x.UnmatchedPolicy)
            .HasEnumAsString(12)
            .HasDefaultValue(UnmatchedPolicy.Fail);
        builder.Property(x => x.ChangeReason).HasMaxLength(500);
        builder.Property(x => x.Direction).HasEnumAsString(15);
        builder.Property(x => x.CreatedAt).IsRequired();
        builder.Property(x => x.UpdatedAt).IsRequired();

        builder.HasOrganizationOwner();
        builder
            .HasOne<MappingSet>()
            .WithMany()
            .HasForeignKey(x => new { x.OrganizationId, x.MappingSetId })
            .HasPrincipalKey(s => new { s.OrganizationId, s.Id })
            .OnDelete(DeleteBehavior.Restrict);
        builder
            .HasOne<MappingSetVersion>()
            .WithMany()
            .HasForeignKey(x => new { x.OrganizationId, x.CopiedFromVersionId })
            .HasPrincipalKey(v => new { v.OrganizationId, v.Id })
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);

        // Entries are removed only when a Draft is discarded (cascade, FRS 6.11).
        //builder
        //    .HasMany(x => x.Entries)
        //    .WithOne()
        //    .HasForeignKey(e => new { e.OrganizationId, e.VersionId })
        //    .HasPrincipalKey(v => new { v.OrganizationId, v.Id })
        //    .OnDelete(DeleteBehavior.Cascade);

        // CHANGE: Restrict instead of Cascade. A version must not take its entries with it by accident; the discard flow deletes the draft's
        // entries explicitly after EnsureDiscardable().
        builder
            .HasMany(x => x.Entries)
            .WithOne()
            .HasForeignKey(e => new { e.OrganizationId, e.VersionId })
            .HasPrincipalKey(v => new { v.OrganizationId, v.Id })
            .OnDelete(DeleteBehavior.Restrict);
        builder
            .Navigation(x => x.Entries)
            .HasField("_entries")
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasIndex(x => new { x.MappingSetId, x.VersionNumber }).IsUnique();
        builder
            .HasIndex(x => x.MappingSetId)
            .IsUnique()
            .HasFilter("status = 'Published'")
            .HasDatabaseName("ux_mapping_set_versions_published");
        builder
            .HasIndex(x => x.MappingSetId)
            .IsUnique()
            .HasFilter("status = 'Draft'")
            .HasDatabaseName("ux_mapping_set_versions_draft");

        builder.IgnoreDomainEvents();
        builder.UseXminConcurrency();
        builder.UseSnakeCaseColumns();
    }
}
