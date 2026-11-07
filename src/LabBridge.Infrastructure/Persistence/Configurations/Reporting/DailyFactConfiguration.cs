using LabBridge.Domain.Features.Reporting.Entities;
using LabBridge.Infrastructure.Persistence.Configurations.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LabBridge.Infrastructure.Persistence.Configurations.Reporting;

/// <summary>Atomic upsert on the unique key. Simulated and real facts are separate rows. numeric(38,18). No patient data.</summary>
public sealed class DailyFactConfiguration : IEntityTypeConfiguration<DailyFact>
{
    public void Configure(EntityTypeBuilder<DailyFact> builder)
    {
        builder.ToTable("daily_facts", "reporting",
                        t =>
                        {
                            t.HasCheckConstraint("ck_daily_facts_count", "fact_count >= 0");
                            // Aggregates are present together or not at all (Add always sets sum, min and max together).
                            t.HasCheckConstraint(
                                "ck_daily_facts_aggregates",
                                "(sum_value IS NULL) = (min_value IS NULL) AND (min_value IS NULL) = (max_value IS NULL)"
                            );
                            t.HasCheckConstraint("ck_daily_facts_range", "min_value IS NULL OR min_value <= max_value");
                        });
        builder.HasDomainAssignedKey();
        builder.Property(x => x.FactDate).HasColumnType("date").IsRequired();
        builder.Property(x => x.FactType).HasMaxLength(100).IsRequired();
        builder.Property(x => x.DimensionsKey).HasMaxLength(300).IsRequired();
        builder.Property(x => x.IsSimulated).IsRequired();
        builder.Property(x => x.DimensionsJson).HasJsonbColumn().IsRequired();
        builder.Property(x => x.FactCount).IsRequired();
        builder.Property(x => x.SumValue).HasPrecision(38, 18);
        builder.Property(x => x.MinValue).HasPrecision(38, 18);
        builder.Property(x => x.MaxValue).HasPrecision(38, 18);
        builder.Property(x => x.HistogramJson).HasJsonbColumn();
        builder.Property(x => x.UpdatedAt).IsRequired();

        builder.HasOrganizationOwner();
        builder
            .HasIndex(x => new
            {
                x.OrganizationId,
                x.FactDate,
                x.FactType,
                x.DimensionsKey,
                x.IsSimulated,
            })
            .IsUnique()
            .HasDatabaseName("ux_daily_facts_key");
        builder.HasIndex(x => x.FactDate); // retention
        builder.UseSnakeCaseColumns();
    }
}
