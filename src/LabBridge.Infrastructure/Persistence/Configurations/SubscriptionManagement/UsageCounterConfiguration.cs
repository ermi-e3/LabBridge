using LabBridge.Domain.Features.SubscriptionManagement.Entities;
using LabBridge.Infrastructure.Persistence.Configurations.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LabBridge.Infrastructure.Persistence.Configurations.SubscriptionManagement;

/// <summary>Atomic upsert on (organization, period, metric key); numeric(38,18).</summary>
public sealed class UsageCounterConfiguration : IEntityTypeConfiguration<UsageCounter>
{
    public void Configure(EntityTypeBuilder<UsageCounter> builder)
    {
        builder.ToTable("usage_counters");
        builder.HasDomainAssignedKey();
        builder.Property(x => x.Period).HasMaxLength(20).IsRequired();
        builder.Property(x => x.MetricKey).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Value).HasPrecision(38, 18).IsRequired();
        builder.Property(x => x.UpdatedAt).IsRequired();

        builder.HasOrganizationOwner();
        builder
            .HasIndex(x => new
            {
                x.OrganizationId,
                x.Period,
                x.MetricKey,
            })
            .IsUnique();
        builder.UseSnakeCaseColumns();
    }
}
