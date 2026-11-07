using LabBridge.Domain.Features.TestCodeMapping.Entities;
using LabBridge.Infrastructure.Persistence.Configurations.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LabBridge.Infrastructure.Persistence.Configurations.TestCodeMapping;

/// <summary>Append-only. numeric(38,18) for clinical values.</summary>
public sealed class MappingResolutionConfiguration : IEntityTypeConfiguration<MappingResolution>
{
    public void Configure(EntityTypeBuilder<MappingResolution> builder)
    {
        builder.ToTable("mapping_resolutions", "testCode_mapping",
            t =>
            {
                t.HasCheckConstraint("ck_mapping_resolutions_failure", "(resolution_type = 'Failed') = (failure_reason IS NOT NULL)");
                t.HasCheckConstraint("ck_mapping_resolutions_conversion", "conversion_applied = (conversion_rule_id IS NOT NULL)");
                t.HasCheckConstraint("ck_mapping_resolutions_line", "line_number >= 0");
            });
        builder.HasDomainAssignedKey();
        builder.Property(x => x.Domain).HasEnumAsString(15);
        builder.Property(x => x.SourceCode).HasMaxLength(100);
        builder.Property(x => x.SourceCodeSystem).HasMaxLength(100);
        builder.Property(x => x.TargetCode).HasMaxLength(100);
        builder.Property(x => x.ResolutionType).HasEnumAsString(12);
        builder.Property(x => x.FailureReason).HasEnumAsString(30);
        builder.Property(x => x.ConversionApplied).IsRequired().HasDefaultValue(false);
        builder.Property(x => x.OriginalNumeric).HasPrecision(38, 18);
        builder.Property(x => x.ConvertedNumeric).HasPrecision(38, 18);

        builder.HasOrganizationOwner();
        builder
            .HasOne<MappingRun>()
            .WithMany()
            .HasForeignKey(x => new { x.OrganizationId, x.RunId })
            .HasPrincipalKey(r => new { r.OrganizationId, r.Id })
            .OnDelete(DeleteBehavior.Restrict);
        builder
            .HasOne<CanonicalConcept>()
            .WithMany()
            .HasForeignKey(x => new { x.OrganizationId, x.ResolvedConceptId })
            .HasPrincipalKey(c => new { c.OrganizationId, c.Id })
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);
        builder
            .HasOne<MappingSetVersion>()
            .WithMany()
            .HasForeignKey(x => new { x.OrganizationId, x.SourceVersionId })
            .HasPrincipalKey(v => new { v.OrganizationId, v.Id })
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);
        builder
            .HasOne<MappingSetVersion>()
            .WithMany()
            .HasForeignKey(x => new { x.OrganizationId, x.TargetVersionId })
            .HasPrincipalKey(v => new { v.OrganizationId, v.Id })
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);
        builder
            .HasOne<UnitConversionRule>()
            .WithMany()
            .HasForeignKey(x => new { x.OrganizationId, x.ConversionRuleId })
            .HasPrincipalKey(r => new { r.OrganizationId, r.Id })
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new { x.RunId, x.LineNumber });
        builder.UseSnakeCaseColumns();
    }
}
