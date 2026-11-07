using LabBridge.Domain.Features.TestCodeMapping.Entities;
using LabBridge.Domain.Features.TestCodeMapping.Enums;
using LabBridge.Infrastructure.Persistence.Configurations.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LabBridge.Infrastructure.Persistence.Configurations.TestCodeMapping;

/// <summary>Decimal precision numeric(38,18) is mandatory for clinical values; binary floating point is never used (BR-MAP unit conversion).</summary>
public sealed class UnitConversionRuleConfiguration : IEntityTypeConfiguration<UnitConversionRule>
{
    public void Configure(EntityTypeBuilder<UnitConversionRule> builder)
    {
        builder.ToTable(
            "unit_conversion_rules",
            t =>
            {
                t.HasCheckConstraint("ck_unit_conversion_rules_units_differ", "from_unit_concept_id <> to_unit_concept_id");
                t.HasCheckConstraint("ck_unit_conversion_rules_multiplier", "multiplier <> 0");
                t.HasCheckConstraint("ck_unit_conversion_rules_rounding", "(rounding_decimal_places IS NULL) = (rounding_mode IS NULL)");
                t.HasCheckConstraint("ck_unit_conversion_rules_places", "rounding_decimal_places IS NULL OR rounding_decimal_places BETWEEN 0 AND 18");
                t.HasCheckConstraint(
                    "ck_unit_conversion_rules_approval",
                    "(status IN ('Approved','Retired')) = (approved_at IS NOT NULL AND approved_by IS NOT NULL)"
                );
                t.HasCheckConstraint(
                    "ck_unit_conversion_rules_retirement",
                    "(status = 'Retired') = (retired_at IS NOT NULL AND retired_by IS NOT NULL)"
                );
                t.HasCheckConstraint("ck_unit_conversion_rules_supersedes", "supersedes_rule_id IS NULL OR supersedes_rule_id <> id");
            }
        );
        builder.HasDomainAssignedKey();
        builder.HasTenantAlternateKey();
        builder.Property(x => x.Multiplier).HasPrecision(38, 18).IsRequired();
        builder.Property(x => x.OffsetValue).HasPrecision(38, 18).IsRequired().HasDefaultValue(0m);
        builder.Property(x => x.RoundingMode).HasEnumAsString(10);
        builder.Property(x => x.EvidenceReference).HasMaxLength(500).IsRequired();
        builder.Property(x => x.Status).HasEnumAsString(10).HasDefaultValue(RuleStatus.Draft);
        builder.Property(x => x.CreatedAt).IsRequired();

        builder.HasOrganizationOwner();
        builder
            .HasOne<CanonicalConcept>()
            .WithMany()
            .HasForeignKey(x => new { x.OrganizationId, x.TestConceptId })
            .HasPrincipalKey(c => new { c.OrganizationId, c.Id })
            .OnDelete(DeleteBehavior.Restrict);
        builder
            .HasOne<CanonicalConcept>()
            .WithMany()
            .HasForeignKey(x => new { x.OrganizationId, x.FromUnitConceptId })
            .HasPrincipalKey(c => new { c.OrganizationId, c.Id })
            .OnDelete(DeleteBehavior.Restrict);
        builder
            .HasOne<CanonicalConcept>()
            .WithMany()
            .HasForeignKey(x => new { x.OrganizationId, x.ToUnitConceptId })
            .HasPrincipalKey(c => new { c.OrganizationId, c.Id })
            .OnDelete(DeleteBehavior.Restrict);
        builder
            .HasOne<UnitConversionRule>()
            .WithMany()
            .HasForeignKey(x => new { x.OrganizationId, x.SupersedesRuleId })
            .HasPrincipalKey(r => new { r.OrganizationId, r.Id })
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasIndex(x => new
            {
                x.OrganizationId,
                x.TestConceptId,
                x.FromUnitConceptId,
                x.ToUnitConceptId,
            })
            .IsUnique()
            .HasFilter("status = 'Approved'")
            .HasDatabaseName("ux_unit_conversion_rules_approved");

        builder.IgnoreDomainEvents();
        builder.UseXminConcurrency();
        builder.UseSnakeCaseColumns();
    }
}
