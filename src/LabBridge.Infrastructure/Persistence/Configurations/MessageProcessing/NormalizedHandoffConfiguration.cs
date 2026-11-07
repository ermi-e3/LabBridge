using LabBridge.Domain.Features.MessageProcessing.Entities;
using LabBridge.Domain.Features.MessageProcessing.Enums;
using LabBridge.Infrastructure.Persistence.Configurations.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LabBridge.Infrastructure.Persistence.Configurations.MessageProcessing;

public sealed class NormalizedHandoffConfiguration : IEntityTypeConfiguration<NormalizedHandoff>
{
    public void Configure(EntityTypeBuilder<NormalizedHandoff> builder)
    {
        builder.ToTable(
            "normalized_handoffs", "message_processing",
            t =>
                t.HasCheckConstraint(
                    "ck_normalized_handoffs_lease",
                    "(status = 'Claimed') = (claimed_by IS NOT NULL AND lease_expires_at IS NOT NULL)"
                )
        );
        builder.HasDomainAssignedKey();
        builder.Property(x => x.Consumer).HasEnumAsString(10);
        builder
            .Property(x => x.Status)
            .HasEnumAsString(10)
            .HasDefaultValue(NormalizedHandoffStatus.Pending);
        builder.Property(x => x.ReceiptSequence).IsRequired();
        builder.Property(x => x.AvailableAt).IsRequired();
        builder.Property(x => x.ClaimedBy).HasMaxLength(100);
        builder.Property(x => x.AttemptCount).IsRequired().HasDefaultValue(0);
        builder.Property(x => x.CreatedAt).IsRequired();
        builder.Property(x => x.UpdatedAt).IsRequired();

        builder.HasOrganizationOwner();
        builder
            .HasOne<NormalizedMessage>()
            .WithOne()
            .HasForeignKey<NormalizedHandoff>(x => new { x.OrganizationId, x.NormalizedMessageId })
            .HasPrincipalKey<NormalizedMessage>(m => new { m.OrganizationId, m.Id })
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.NormalizedMessageId).IsUnique();
        builder.HasIndex(x => new
        {
            x.Consumer,
            x.Status,
            x.AvailableAt,
            x.ReceiptSequence,
        });

        builder.HasIndex(x => new { x.Consumer, x.ReceiptSequence })
            .HasFilter("status = 'Pending'")
            .HasDatabaseName("ix_normalized_handoffs_pending");

        builder.HasIndex(x => x.LeaseExpiresAt).HasFilter("status = 'Claimed'");

        builder.IgnoreDomainEvents();
        builder.UseXminConcurrency();
        builder.UseSnakeCaseColumns();
    }
}
