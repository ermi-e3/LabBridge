using LabBridge.Domain.Features.DeliveryManagement.Entities;
using LabBridge.Domain.Features.DeliveryManagement.Enums;
using LabBridge.Domain.Features.InstrumentManagement.Entities;
using LabBridge.Domain.Features.OrderResultManagement.Entities;
using LabBridge.Infrastructure.Persistence.Configurations.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LabBridge.Infrastructure.Persistence.Configurations.DeliveryManagement;

/// <summary>One item per delivery request; (OrganizationId, IdempotencyKey) unique. Lease fields set exactly while InProgress.</summary>
public sealed class DeliveryItemConfiguration : IEntityTypeConfiguration<DeliveryItem>
{
    public void Configure(EntityTypeBuilder<DeliveryItem> builder)
    {
        builder.ToTable(
            "delivery_items",
            t =>
            {
                t.HasCheckConstraint(
                    "ck_delivery_items_hold",
                    "hold_reason IS NULL OR state IN ('Pending','RetryScheduled')"
                );
                t.HasCheckConstraint(
                    "ck_delivery_items_quarantine",
                    "(quarantined_from_state IS NOT NULL) = (state = 'Quarantined')"
                );
                t.HasCheckConstraint(
                    "ck_delivery_items_lease",
                    "(state = 'InProgress') = (claimed_by IS NOT NULL AND lease_expires_at IS NOT NULL)"
                );
                // added 
                t.HasCheckConstraint("ck_delivery_items_counters", "attempt_count >= 0 AND payload_version >= 1 AND receipt_sequence > 0");
                t.HasCheckConstraint("ck_delivery_items_retry", "state <> 'RetryScheduled' OR next_attempt_at IS NOT NULL");
            }
        );
        builder.HasDomainAssignedKey();
        builder.HasTenantAlternateKey();
        builder.Property(x => x.SubjectType).HasEnumAsString(10);
        builder.Property(x => x.PayloadVersion).IsRequired();
        builder.Property(x => x.IdempotencyKey).HasMaxLength(200).IsRequired();
        builder.Property(x => x.State).HasEnumAsString(20).HasDefaultValue(DeliveryState.Pending);
        builder.Property(x => x.HoldReason).HasEnumAsString(30);
        builder.Property(x => x.QuarantinedFromState).HasEnumAsString(20);
        builder.Property(x => x.AttemptCount).IsRequired().HasDefaultValue(0);
        builder.Property(x => x.ReceiptSequence).IsRequired();
        builder.Property(x => x.ClaimedBy).HasMaxLength(100);
        builder.Property(x => x.IsSimulated).IsRequired();
        builder.Property(x => x.CreatedAt).IsRequired();
        builder.Property(x => x.UpdatedAt).IsRequired();

        builder.HasOrganizationOwner();
        builder
            .HasOne<Route>()
            .WithMany()
            .HasForeignKey(x => new { x.OrganizationId, x.RouteId })
            .HasPrincipalKey(r => new { r.OrganizationId, r.Id })
            .OnDelete(DeleteBehavior.Restrict);
        builder
            .HasOne<Connector>()
            .WithMany()
            .HasForeignKey(x => new { x.OrganizationId, x.ConnectorId })
            .HasPrincipalKey(c => new { c.OrganizationId, c.Id })
            .OnDelete(DeleteBehavior.Restrict);
        builder
            .HasOne<DeliveryRequest>()
            .WithOne()
            .HasForeignKey<DeliveryItem>(x => new { x.OrganizationId, x.DeliveryRequestId })
            .HasPrincipalKey<DeliveryRequest>(r => new { r.OrganizationId, r.Id })
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasMany(x => x.Attempts)
            .WithOne()
            .HasForeignKey(a => new { a.OrganizationId, a.DeliveryId })
            .HasPrincipalKey(i => new { i.OrganizationId, i.Id })
            .OnDelete(DeleteBehavior.Restrict);
        builder
            .Navigation(x => x.Attempts)
            .HasField("_attempts")
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasIndex(x => x.DeliveryRequestId).IsUnique();
        builder.HasIndex(x => new { x.OrganizationId, x.IdempotencyKey }).IsUnique();
        builder.HasIndex(x => new
        {
            x.State,
            x.NextAttemptAt,
            x.ReceiptSequence,
        });
        builder.HasIndex(x => new
        {
            x.OrganizationId,
            x.State,
            x.CreatedAt,
        });

        builder.HasIndex(x => x.ReceiptSequence)
            .HasFilter("state IN ('Pending','RetryScheduled') AND hold_reason IS NULL")
            .HasDatabaseName("ix_delivery_items_dispatchable");

        builder.HasIndex(x => x.RouteId).HasFilter("state = 'InProgress'").HasDatabaseName("ix_delivery_items_route_in_progress");

        builder.HasIndex(x => new { x.SubjectType, x.SubjectId });
        builder.HasIndex(x => x.LeaseExpiresAt).HasFilter("state = 'InProgress'");

        builder.IgnoreDomainEvents();
        builder.UseXminConcurrency();
        builder.UseSnakeCaseColumns();
    }
}
