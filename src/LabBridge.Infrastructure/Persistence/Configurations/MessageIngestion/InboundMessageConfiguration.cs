using LabBridge.Domain.Features.InstrumentManagement.Entities;
using LabBridge.Domain.Features.MessageIngestion.Entities;
using LabBridge.Infrastructure.Persistence.Configurations.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LabBridge.Infrastructure.Persistence.Configurations.MessageIngestion;

/// <summary>
/// Message metadata. Composite tenant FKs to connector and payload. ReceiptSequence is database-assigned from a sequence
/// (the migration must create inbound_message_receipt_seq). Append-only history is aggregate-owned.
/// </summary>
public sealed class InboundMessageConfiguration : IEntityTypeConfiguration<InboundMessage>
{
    public void Configure(EntityTypeBuilder<InboundMessage> builder)
    {
        //builder.ToTable(
        //    "inbound_messages",
        //    t =>
        //    {
        //        t.HasCheckConstraint(
        //            "ck_inbound_messages_original_required",
        //            "receipt_outcome = 'Accepted' OR original_message_id IS NOT NULL"
        //        );
        //        t.HasCheckConstraint(
        //            "ck_inbound_messages_dedup_hash",
        //            "content_hash ~ '^[0-9a-f]{64}$'"
        //        );
        //    }
        //);
        // added
        builder.ToTable("inbound_messages", "message_ingestion", t =>
        {
            t.HasCheckConstraint("ck_inbound_messages_original_required",
                "receipt_outcome = 'Accepted' OR original_message_id IS NOT NULL");
            t.HasCheckConstraint("ck_inbound_messages_content_hash", "content_hash ~ '^[0-9a-f]{64}$'");   // was ck_inbound_messages_dedup_hash
            t.HasCheckConstraint("ck_inbound_messages_counters", "size_bytes > 0 AND connector_config_version >= 1");
        });
        builder.HasDomainAssignedKey();
        builder.HasTenantAlternateKey();
        builder.Property(x => x.ConnectorConfigVersion).IsRequired();
        builder.Property(x => x.CorrelationId).IsRequired();
        builder
            .Property(x => x.ReceiptSequence)
            .ValueGeneratedOnAdd()
            .HasDefaultValueSql("nextval('inbound_message_receipt_seq')"); //inbound_message_receipt_seq
        builder.Property(x => x.ReceivedAt).IsRequired();
        builder.Property(x => x.Status).HasEnumAsString(20);
        builder.Property(x => x.StatusReason).HasMaxLength(500);
        builder.Property(x => x.MessageKind).HasEnumAsString(20);
        builder.Property(x => x.IsSimulated).IsRequired();
        builder.Property(x => x.TransportType).HasMaxLength(50).IsRequired();
        builder.Property(x => x.RemoteEndpoint).HasMaxLength(300);
        builder.Property(x => x.ContentHash).HasMaxLength(64).IsFixedLength().IsRequired();
        builder.Property(x => x.SizeBytes).IsRequired();
        builder.Property(x => x.EncodingHint).HasMaxLength(50);
        builder.Property(x => x.DedupKey).HasMaxLength(128).IsRequired();
        builder.Property(x => x.DedupKeySource).HasEnumAsString(20);
        builder.Property(x => x.ReceiptOutcome).HasEnumAsString(20);
        //builder.Property(x => x.ReceiptReplyDescriptor).HasJsonbColumn();
        builder.Property(x => x.ReceiptReplyDescriptor).HasJsonbColumn();
        builder.Property(x => x.TraceId).HasMaxLength(64);
        builder.Property(x => x.CreatedAt).IsRequired();
        builder.Property(x => x.UpdatedAt).IsRequired();

        builder.HasOrganizationOwner();
        builder
            .HasOne<Connector>()
            .WithMany()
            .HasForeignKey(x => new { x.OrganizationId, x.ConnectorId })
            .HasPrincipalKey(c => new { c.OrganizationId, c.Id })
            .OnDelete(DeleteBehavior.Restrict);
        builder
            .HasOne<RawPayload>()
            .WithMany()
            .HasForeignKey(x => new { x.OrganizationId, x.PayloadId })
            .HasPrincipalKey(p => new { p.OrganizationId, p.Id })
            .OnDelete(DeleteBehavior.Restrict);
        builder
            .HasOne<InterfaceProfileVersion>()
            .WithMany()
            .HasForeignKey(x => x.ProfileVersionId)
            .OnDelete(DeleteBehavior.Restrict);
        builder
            .HasOne<InboundMessage>()
            .WithMany()
            .HasForeignKey(x => new { x.OrganizationId, x.OriginalMessageId })
            .HasPrincipalKey(m => new { m.OrganizationId, m.Id })
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasMany(x => x.StatusHistory)
            .WithOne()
            .HasForeignKey(h => new { h.OrganizationId, h.MessageId })
            .HasPrincipalKey(m => new { m.OrganizationId, m.Id })
            .OnDelete(DeleteBehavior.Restrict);
        builder
            .Navigation(x => x.StatusHistory)
            .HasField("_statusHistory")
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasIndex(x => x.ReceiptSequence).IsUnique();
        builder.HasIndex(x => new { x.OrganizationId, x.ReceivedAt }).IsDescending(false, true);
        builder.HasIndex(x => new
        {
            x.OrganizationId,
            x.Status,
            x.ReceivedAt,
        });
        builder.HasIndex(x => new
        {
            x.OrganizationId,
            x.ConnectorId,
            x.ReceivedAt,
        });
        builder.HasIndex(x => new { x.OrganizationId, x.ContentHash });
        builder.HasIndex(x => x.CorrelationId);

        builder.IgnoreDomainEvents();
        builder.UseXminConcurrency();
        builder.UseSnakeCaseColumns();
    }
}
