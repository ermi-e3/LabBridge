using LabBridge.Domain.Features.MessageIngestion.Entities;
using LabBridge.Domain.Features.MessageIngestion.Enums;
using LabBridge.Infrastructure.Persistence.Configurations.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LabBridge.Infrastructure.Persistence.Configurations.MessageIngestion;

/// <summary>One hand-off per message. Pending/Held lookups are ordered by receipt sequence (BR-ING-010).</summary>
public sealed class InboundProcessingHandoffConfiguration
    : IEntityTypeConfiguration<InboundProcessingHandoff>
{
    public void Configure(EntityTypeBuilder<InboundProcessingHandoff> builder)
    {
        builder.ToTable(
            "inbound_processing_handoffs", "message_ingestion",
            t =>
            {
                t.HasCheckConstraint(
                    "ck_handoffs_claim",
                    "(status = 'Claimed') = (claimed_by IS NOT NULL AND lease_expires_at IS NOT NULL)"
                );
                t.HasCheckConstraint(
                    "ck_handoffs_hold",
                    "(status = 'Held') = (hold_reason IS NOT NULL)"
                );
            }
        );
        builder.HasDomainAssignedKey();
        builder.Property(x => x.Status).HasEnumAsString(15).HasDefaultValue(HandoffStatus.Pending);
        builder.Property(x => x.HoldReason).HasMaxLength(50);
        builder.Property(x => x.ReceiptSequence).IsRequired();
        builder.Property(x => x.AvailableAt).IsRequired();
        builder.Property(x => x.ClaimedBy).HasMaxLength(100);
        builder.Property(x => x.AttemptCount).IsRequired().HasDefaultValue(0);
        builder.Property(x => x.CreatedAt).IsRequired();
        builder.Property(x => x.UpdatedAt).IsRequired();

        builder.HasOrganizationOwner();
        builder
            .HasOne<InboundMessage>()
            .WithOne()
            .HasForeignKey<InboundProcessingHandoff>(x => new { x.OrganizationId, x.MessageId })
            .HasPrincipalKey<InboundMessage>(m => new { m.OrganizationId, m.Id })
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.MessageId).IsUnique();
        builder.HasIndex(x => new
        {
            x.Status,
            x.AvailableAt,
            x.ReceiptSequence,
        });
        builder.HasIndex(x => new
        {
            x.OrganizationId,
            x.Status,
            x.ReceiptSequence,
        });

        builder.HasIndex(x => x.ReceiptSequence)
            .HasFilter("status = 'Pending'")
            .HasDatabaseName("ix_inbound_handoffs_pending"); // the explicit unique index on MessageId already covers the 1:1 relationship
        builder.HasIndex(x => x.LeaseExpiresAt).HasFilter("status = 'Claimed'");

        builder.IgnoreDomainEvents();
        builder.UseXminConcurrency();
        builder.UseSnakeCaseColumns();
    }
}
