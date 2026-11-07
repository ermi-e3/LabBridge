using LabBridge.Domain.Features.SubscriptionManagement.Entities;
using LabBridge.Infrastructure.Persistence.Configurations.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LabBridge.Infrastructure.Persistence.Configurations.SubscriptionManagement;

/// <summary>Exactly one subscription per organization (unique OrganizationId). Never deleted.</summary>
public sealed class SubscriptionConfiguration : IEntityTypeConfiguration<Subscription>
{
    public void Configure(EntityTypeBuilder<Subscription> builder)
    {
        builder.ToTable("subscriptions", "subscription_management");
        builder.HasDomainAssignedKey();
        builder.HasTenantAlternateKey();
        builder.Property(x => x.Status).HasEnumAsString(10);
        builder.Property(x => x.PeriodAnchorDate).HasColumnType("date").IsRequired();
        builder.Property(x => x.ChangedAt).IsRequired();

        builder.HasOrganizationOwner();
        builder
            .HasOne<Plan>()
            .WithMany()
            .HasForeignKey(x => x.PlanId)
            .OnDelete(DeleteBehavior.Restrict);
        builder
            .HasMany(x => x.History)
            .WithOne()
            .HasForeignKey(h => new { h.OrganizationId, h.SubscriptionId })
            .HasPrincipalKey(s => new { s.OrganizationId, s.Id })
            .OnDelete(DeleteBehavior.Restrict);
        builder
            .Navigation(x => x.History)
            .HasField("_history")
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasIndex(x => x.OrganizationId).IsUnique();
        builder.HasIndex(x => x.PlanId);

        builder.IgnoreDomainEvents();
        builder.UseXminConcurrency();
        builder.UseSnakeCaseColumns();
    }
}
