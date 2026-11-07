using LabBridge.Domain.Features.SubscriptionManagement.Entities;
using LabBridge.Infrastructure.Persistence.Configurations.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LabBridge.Infrastructure.Persistence.Configurations.SubscriptionManagement;

public sealed class SubscriptionHistoryConfiguration : IEntityTypeConfiguration<SubscriptionHistory>
{
    public void Configure(EntityTypeBuilder<SubscriptionHistory> builder)
    {
        builder.ToTable("subscription_history", "subscription_management",
            t =>
                // The first row has neither a previous plan nor a previous status; every later row has both.
                 t.HasCheckConstraint("ck_subscription_history_from", "(from_plan_id IS NULL) = (from_status IS NULL)"));
        builder.HasDomainAssignedKey();
        builder.Property(x => x.FromStatus).HasEnumAsString(10);
        builder.Property(x => x.ToStatus).HasEnumAsString(10);
        builder.Property(x => x.Reason).HasMaxLength(500);
        builder.Property(x => x.ChangedAt).IsRequired();

        builder.HasOrganizationOwner();
        builder
            .HasOne<Plan>()
            .WithMany()
            .HasForeignKey(x => x.FromPlanId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);
        builder
            .HasOne<Plan>()
            .WithMany()
            .HasForeignKey(x => x.ToPlanId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new { x.SubscriptionId, x.ChangedAt });
        builder.UseSnakeCaseColumns();
    }
}
