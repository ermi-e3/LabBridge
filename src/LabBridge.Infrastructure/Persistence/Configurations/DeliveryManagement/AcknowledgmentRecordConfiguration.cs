using LabBridge.Domain.Features.DeliveryManagement.Entities;
using LabBridge.Domain.Features.MessageIngestion.Entities;
using LabBridge.Infrastructure.Persistence.Configurations.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LabBridge.Infrastructure.Persistence.Configurations.DeliveryManagement;

/// <summary>One record per inbound acknowledgment message (unique MessageId); attempt link is nullable until matched.</summary>
public sealed class AcknowledgmentRecordConfiguration
    : IEntityTypeConfiguration<AcknowledgmentRecord>
{
    public void Configure(EntityTypeBuilder<AcknowledgmentRecord> builder)
    {
        //builder.ToTable("acknowledgment_records");

        // added check constraints to ensure that match_status and attempt_id/matched_at are consistent
        builder.ToTable("acknowledgment_records", t =>
        {
            t.HasCheckConstraint("ck_acknowledgment_records_match", "(match_status = 'Matched') = (attempt_id IS NOT NULL)");
            t.HasCheckConstraint("ck_acknowledgment_records_matched_at", "(match_status = 'Matched') = (matched_at IS NOT NULL)");
        });
        builder.HasDomainAssignedKey();

        builder.Property(x => x.AcknowledgesControlId).HasMaxLength(100);
        builder.Property(x => x.AckCode).HasMaxLength(50);
        builder.Property(x => x.AcceptanceCode).HasMaxLength(50);
        builder.Property(x => x.ErrorCategory).HasMaxLength(50);
        builder.Property(x => x.MatchStatus).HasEnumAsString(10);
        builder.Property(x => x.ReceivedAt).IsRequired();

        builder.HasOrganizationOwner();
        builder
            .HasOne<InboundMessage>()
            .WithMany()
            .HasForeignKey(x => new { x.OrganizationId, x.MessageId })
            .HasPrincipalKey(m => new { m.OrganizationId, m.Id })
            .OnDelete(DeleteBehavior.Restrict);
        builder
            .HasOne<DeliveryAttempt>()
            .WithMany()
            .HasForeignKey(x => new { x.OrganizationId, x.AttemptId })
            .HasPrincipalKey(a => new { a.OrganizationId, a.Id })
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.MessageId).IsUnique();
        builder.HasIndex(x => new { x.OrganizationId, x.AcknowledgesControlId });
        builder.HasIndex(x => new
        {
            x.OrganizationId,
            x.MatchStatus,
            x.ReceivedAt,
        });

        builder.IgnoreDomainEvents();
        builder.UseXminConcurrency();
        builder.UseSnakeCaseColumns();
    }
}
