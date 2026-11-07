using LabBridge.Domain.Features.InstrumentManagement.Entities;
using LabBridge.Domain.Features.OrderResultManagement.Entities;
using LabBridge.Domain.Features.OrderResultManagement.Enums;
using LabBridge.Infrastructure.Persistence.Configurations.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LabBridge.Infrastructure.Persistence.Configurations.OrderResultManagement;

/// <summary>Subject reference is polymorphic (Order or Result): validated by the application, not by a foreign key (FRS DB-WFL-007).</summary>
public sealed class DeliveryRequestConfiguration : IEntityTypeConfiguration<DeliveryRequest>
{
    public void Configure(EntityTypeBuilder<DeliveryRequest> builder)
    {
        //builder.ToTable(
        //    "delivery_requests",
        //    t =>
        //        t.HasCheckConstraint(
        //            "ck_delivery_requests_lease",
        //            "(status = 'Claimed') = (claimed_by IS NOT NULL AND lease_expires_at IS NOT NULL)"
        //        )
        //);
        builder.ToTable(
                    "delivery_requests",
                    t =>
                    {
                        t.HasCheckConstraint(
                            "ck_delivery_requests_lease",
                            "(status = 'Claimed') = (claimed_by IS NOT NULL AND lease_expires_at IS NOT NULL)"
                        );
                        t.HasCheckConstraint("ck_delivery_requests_counters", "attempt_count >= 0 AND payload_version >= 1 AND receipt_sequence > 0");
                    }
                );
        builder.HasDomainAssignedKey();
        builder.HasTenantAlternateKey();
        builder.Property(x => x.SubjectType).HasEnumAsString(10);
        builder.Property(x => x.PayloadVersion).IsRequired();
        builder
            .Property(x => x.Status)
            .HasEnumAsString(10)
            .HasDefaultValue(DeliveryRequestStatus.Pending);
        builder.Property(x => x.ReceiptSequence).IsRequired();
        builder.Property(x => x.AvailableAt).IsRequired();
        builder.Property(x => x.ClaimedBy).HasMaxLength(100);
        builder.Property(x => x.AttemptCount).IsRequired().HasDefaultValue(0);
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
            .HasIndex(x => new
            {
                x.SubjectType,
                x.SubjectId,
                x.PayloadVersion,
            })
            .IsUnique();
        builder.HasIndex(x => new
        {
            x.Status,
            x.AvailableAt,
            x.ReceiptSequence,
        });
        builder.HasIndex(x => x.LeaseExpiresAt).HasFilter("status = 'Claimed'");

        builder
            .HasIndex(x => x.ReceiptSequence)
            .HasFilter("status = 'Pending'")
            .HasDatabaseName("ix_delivery_requests_pending");

        builder.IgnoreDomainEvents();
        builder.UseXminConcurrency();
        builder.UseSnakeCaseColumns();
    }
}
