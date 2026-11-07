using LabBridge.Domain.Features.Monitoring.Entities;
using LabBridge.Domain.Features.Monitoring.Enums;
using LabBridge.Domain.Features.OrganizationManagement.Entities;
using LabBridge.Infrastructure.Persistence.Configurations.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LabBridge.Infrastructure.Persistence.Configurations.Monitoring;

/// <summary>
/// OrganizationId is nullable (platform scope). The FRS partial unique (organization, rule, scope key) WHERE Open/Acknowledged is expressed as two partial
/// indexes (organization present / absent) because NULLs are distinct in unique indexes.
/// </summary>
public sealed class AlertConfiguration : IEntityTypeConfiguration<Alert>
{
    public void Configure(EntityTypeBuilder<Alert> builder)
    {
        builder.ToTable("alerts", "monitoring",
                t =>
                {
                    t.HasCheckConstraint("ck_alerts_resolve_reason", "status <> 'Resolved' OR resolve_reason IS NOT NULL");
                    t.HasCheckConstraint("ck_alerts_resolved_at", "(status = 'Resolved') = (resolved_at IS NOT NULL)");
                    t.HasCheckConstraint("ck_alerts_acknowledged", "status <> 'Acknowledged' OR acknowledged_at IS NOT NULL");
                    t.HasCheckConstraint("ck_alerts_counts", "occurrence_count >= 1 AND last_seen_at >= opened_at");
                }
            );
        builder.HasDomainAssignedKey();
        builder.Property(x => x.ScopeKey).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Severity).HasEnumAsString(10);
        builder.Property(x => x.Status).HasEnumAsString(15).HasDefaultValue(AlertStatus.Open);
        builder.Property(x => x.OccurrenceCount).IsRequired().HasDefaultValue(1);
        builder.Property(x => x.AffectedObjectsJson).HasJsonbColumn();
        builder.Property(x => x.IsSimulated).IsRequired();
        builder.Property(x => x.OpenedAt).IsRequired();
        builder.Property(x => x.LastSeenAt).IsRequired();
        builder.Property(x => x.ResolveReason).HasEnumAsString(20);
        builder.Property(x => x.ResolveNote).HasMaxLength(500);

        builder
            .HasOne<Organization>()
            .WithMany()
            .HasForeignKey(x => x.OrganizationId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);
        builder
            .HasOne<AlertRule>()
            .WithMany()
            .HasForeignKey(x => x.AlertRuleId)
            .OnDelete(DeleteBehavior.Restrict);
        builder
            .HasMany(x => x.History)
            .WithOne()
            .HasForeignKey(h => h.AlertId)
            .OnDelete(DeleteBehavior.Restrict);
        builder
            .Navigation(x => x.History)
            .HasField("_history")
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder
            .HasIndex(x => new
            {
                x.OrganizationId,
                x.AlertRuleId,
                x.ScopeKey,
            })
            .IsUnique()
            .HasFilter("status IN ('Open','Acknowledged') AND organization_id IS NOT NULL")
            .HasDatabaseName("ux_alerts_active_org");
        builder
            .HasIndex(x => new { x.AlertRuleId, x.ScopeKey })
            .IsUnique()
            .HasFilter("status IN ('Open','Acknowledged') AND organization_id IS NULL")
            .HasDatabaseName("ux_alerts_active_platform");
        builder.HasIndex(x => new
        {
            x.OrganizationId,
            x.Status,
            x.OpenedAt,
        });

        builder.IgnoreDomainEvents();
        builder.UseXminConcurrency();
        builder.UseSnakeCaseColumns();
    }
}
