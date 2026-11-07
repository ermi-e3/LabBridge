using LabBridge.Domain.Features.OrderResultManagement.Entities;
using LabBridge.Domain.Features.TestCodeMapping.Entities;
using LabBridge.Infrastructure.Persistence.Configurations.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LabBridge.Infrastructure.Persistence.Configurations.OrderResultManagement;

/// <summary>Original value and converted value are stored side by side; numerics use numeric(38,18). Flags persisted as text[].</summary>
public sealed class ObservationConfiguration : IEntityTypeConfiguration<Observation>
{
    public void Configure(EntityTypeBuilder<Observation> builder)
    {
        //builder.ToTable("lab_observation");
        builder.ToTable(
                    "lab_observation",
                    t =>
                    {
                        t.HasCheckConstraint("ck_lab_observation_line", "line_number >= 1");
                        t.HasCheckConstraint("ck_lab_observation_conversion", "conversion_applied = (conversion_rule_id IS NOT NULL)");
                    }
                );
        builder.HasDomainAssignedKey();
        builder.HasTenantAlternateKey();
        builder.Property(x => x.LineNumber).IsRequired();
        builder.Property(x => x.ExternalTestCode).HasMaxLength(100).IsRequired();
        builder.Property(x => x.ExternalCodeSystem).HasMaxLength(100);
        builder.Property(x => x.CanonicalTestCode).HasMaxLength(100);
        builder.Property(x => x.DestinationTestCode).HasMaxLength(100);
        builder.Property(x => x.ValueType).HasMaxLength(30).IsRequired();
        builder.Property(x => x.OriginalValue).HasMaxLength(2000).IsRequired();
        builder.Property(x => x.NumericValue).HasPrecision(38, 18);
        builder.Property(x => x.OriginalUnit).HasMaxLength(100);
        builder.Property(x => x.CanonicalUnit).HasMaxLength(100);
        builder.Property(x => x.DestinationUnit).HasMaxLength(100);
        builder.Property(x => x.DestinationValueText).HasMaxLength(2000);
        builder.Property(x => x.DestinationValueNumeric).HasPrecision(38, 18);
        builder.Property(x => x.ConversionApplied).IsRequired().HasDefaultValue(false);
        builder.Property(x => x.InstrumentReferenceRangeText).HasMaxLength(200);
        builder.Property(x => x.ObservationStatus).HasMaxLength(30);
        builder.Property(x => x.ObservedAtRaw).HasMaxLength(100);

        builder.Ignore(x => x.InstrumentFlags);
        builder
            .Property<List<string>>("_instrumentFlags")
            .HasColumnName("instrument_flags")
            .HasColumnType("text[]")
            .IsRequired();

        builder.HasOrganizationOwner();
        builder
            .HasOne<OrderLine>()
            .WithMany()
            .HasForeignKey(x => new { x.OrganizationId, x.OrderLineId })
            .HasPrincipalKey(l => new { l.OrganizationId, l.Id })
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);
        builder
            .HasOne<UnitConversionRule>()
            .WithMany()
            .HasForeignKey(x => new { x.OrganizationId, x.ConversionRuleId })
            .HasPrincipalKey(r => new { r.OrganizationId, r.Id })
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new { x.ResultId, x.LineNumber }).IsUnique();
        builder.UseSnakeCaseColumns();
    }
}
