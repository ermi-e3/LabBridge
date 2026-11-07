using LabBridge.Domain.Features.Monitoring.Entities;
using LabBridge.Infrastructure.Persistence.Configurations.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LabBridge.Infrastructure.Persistence.Configurations.Monitoring;

/// <summary>Written by atomic upsert on the unique key; numeric(38,18).</summary>
public sealed class MetricSampleConfiguration : IEntityTypeConfiguration<MetricSample>
{
    public void Configure(EntityTypeBuilder<MetricSample> builder)
    {
        //builder.ToTable("metric_samples");
        builder.ToTable("metric_samples", t => t.HasCheckConstraint("ck_metric_samples_counter", "kind <> 'Counter' OR value >= 0"));  
        builder.HasDomainAssignedKey();
        builder.Property(x => x.MetricName).HasMaxLength(100).IsRequired();
        builder.Property(x => x.DimensionsKey).HasMaxLength(300).IsRequired();
        builder.Property(x => x.BucketStart).IsRequired();
        builder.Property(x => x.Kind).HasEnumAsString(10);
        builder.Property(x => x.Value).HasPrecision(38, 18).IsRequired();
        builder.Property(x => x.UpdatedAt).IsRequired();

        builder.HasOrganizationOwner();
        builder
            .HasIndex(x => new
            {
                x.OrganizationId,
                x.MetricName,
                x.DimensionsKey,
                x.BucketStart,
            })
            .IsUnique()
            .HasDatabaseName("ux_metric_samples_key");
        builder.HasIndex(x => x.BucketStart); // retention
        builder.UseSnakeCaseColumns();
    }
}
