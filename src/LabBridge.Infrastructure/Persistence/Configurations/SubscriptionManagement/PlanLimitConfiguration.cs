using LabBridge.Domain.Features.SubscriptionManagement.Entities;
using LabBridge.Infrastructure.Persistence.Configurations.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LabBridge.Infrastructure.Persistence.Configurations.SubscriptionManagement;

public sealed class PlanLimitConfiguration : IEntityTypeConfiguration<PlanLimit>
{
    public void Configure(EntityTypeBuilder<PlanLimit> builder)
    {
        builder.ToTable("plan_limits", "subscription_management",
            t =>
            {
                t.HasCheckConstraint("ck_plan_limits_quantity_value", "(kind = 'Quantity') = (limit_value IS NOT NULL)");
                t.HasCheckConstraint("ck_plan_limits_value_range", "limit_value IS NULL OR limit_value >= 0");
                t.HasCheckConstraint("ck_plan_limits_feature_flag", "kind = 'Feature' OR NOT feature_enabled");
                t.HasCheckConstraint("ck_plan_limits_message_volume_block", "NOT (is_message_volume AND enforcement_mode = 'Block')");
            });
        builder.HasDomainAssignedKey();
        builder.Property(x => x.LimitKey).HasMaxLength(100).IsRequired();
        builder.Property(x => x.LimitValue).HasPrecision(38, 18);
        builder.Property(x => x.FeatureEnabled).IsRequired().HasDefaultValue(false);
        builder.Property(x => x.EnforcementMode).HasEnumAsString(10);

        builder.Property(x => x.Kind).HasEnumAsString(10);
        builder
            .HasOne<LimitDefinition>()
            .WithMany()
            .HasForeignKey(x => new { x.LimitKey, x.Kind, x.IsMessageVolume })
            .HasPrincipalKey(d => new { d.LimitKey, d.Kind, d.IsMessageVolume })
            .OnDelete(DeleteBehavior.Restrict);
        //builder
        //    .HasOne<LimitDefinition>()
        //    .WithMany()
        //    .HasForeignKey(x => x.LimitKey)
        //    .OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(x => new { x.PlanId, x.LimitKey }).IsUnique();
        // "Block" is not permitted for message-volume limits: the cross-table rule is enforced by the domain (PlanLimit) and a migration trigger.

        builder.UseSnakeCaseColumns();
    }
}
