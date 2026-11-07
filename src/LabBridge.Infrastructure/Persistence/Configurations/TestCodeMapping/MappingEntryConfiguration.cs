using LabBridge.Domain.Features.TestCodeMapping.Entities;
using LabBridge.Infrastructure.Persistence.Configurations.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LabBridge.Infrastructure.Persistence.Configurations.TestCodeMapping;

/// <summary>
/// The FRS unique indexes with COALESCE(code_system,'') are expressed as pairs of partial indexes (code system present / absent).
/// </summary>
public sealed class MappingEntryConfiguration : IEntityTypeConfiguration<MappingEntry>
{
    public void Configure(EntityTypeBuilder<MappingEntry> builder)
    {
        builder.ToTable("mapping_entries", "testCode_mapping");
        builder.HasDomainAssignedKey();
        builder.Property(x => x.Direction).HasEnumAsString(15);
        builder.Property(x => x.ExternalCode).HasMaxLength(100).IsRequired();
        builder.Property(x => x.ExternalCodeSystem).HasMaxLength(100);
        builder.Property(x => x.Notes).HasMaxLength(500);
        builder.Property(x => x.CreatedAt).IsRequired();
        builder.Property(x => x.UpdatedAt).IsRequired();

        builder.HasOrganizationOwner();
        builder
            .HasOne<CanonicalConcept>()
            .WithMany()
            .HasForeignKey(x => new { x.OrganizationId, x.ConceptId })
            .HasPrincipalKey(c => new { c.OrganizationId, c.Id })
            .OnDelete(DeleteBehavior.Restrict);
        builder
            .HasOne<CanonicalConcept>()
            .WithMany()
            .HasForeignKey(x => new { x.OrganizationId, x.TargetUnitConceptId })
            .HasPrincipalKey(c => new { c.OrganizationId, c.Id })
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasIndex(x => new
            {
                x.VersionId,
                x.ExternalCode,
                x.ExternalCodeSystem,
            })
            .IsUnique()
            .HasFilter("direction = 'ToCanonical' AND external_code_system IS NOT NULL")
            .HasDatabaseName("ux_mapping_entries_code_system");
        builder
            .HasIndex(x => new { x.VersionId, x.ExternalCode })
            .IsUnique()
            .HasFilter("direction = 'ToCanonical' AND external_code_system IS NULL")
            .HasDatabaseName("ux_mapping_entries_code");
        builder
            .HasIndex(x => new { x.VersionId, x.ConceptId })
            .IsUnique()
            .HasFilter("direction = 'FromCanonical'")
            .HasDatabaseName("ux_mapping_entries_concept");

        builder.UseXminConcurrency();
        builder.UseSnakeCaseColumns();
    }
}
