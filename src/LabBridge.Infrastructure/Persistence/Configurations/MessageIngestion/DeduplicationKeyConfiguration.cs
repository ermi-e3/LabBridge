using LabBridge.Domain.Features.InstrumentManagement.Entities;
using LabBridge.Domain.Features.MessageIngestion.Entities;
using LabBridge.Infrastructure.Persistence.Configurations.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LabBridge.Infrastructure.Persistence.Configurations.MessageIngestion;

/// <summary>
/// The unique (organization, connector, key) constraint is the atomic dedup claim (BR-ING-008): a losing insert raises a unique violation
/// that the infrastructure translates into the duplicate path.
/// </summary>
public sealed class DeduplicationKeyConfiguration : IEntityTypeConfiguration<DeduplicationKey>
{
    public void Configure(EntityTypeBuilder<DeduplicationKey> builder)
    {
        builder.ToTable("deduplication_keys", "message_ingestion");
        builder.HasDomainAssignedKey();
        builder.Property(x => x.DedupKey).HasMaxLength(128).IsRequired();
        builder.Property(x => x.ContentHash).HasMaxLength(64).IsFixedLength().IsRequired();
        builder.Property(x => x.FirstSeenAt).IsRequired();

        builder.HasOrganizationOwner();
        builder
            .HasOne<Connector>()
            .WithMany()
            .HasForeignKey(x => new { x.OrganizationId, x.ConnectorId })
            .HasPrincipalKey(c => new { c.OrganizationId, c.Id })
            .OnDelete(DeleteBehavior.Restrict);
        builder
            .HasOne<InboundMessage>()
            .WithMany()
            .HasForeignKey(x => new { x.OrganizationId, x.FirstMessageId })
            .HasPrincipalKey(m => new { m.OrganizationId, m.Id })
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasIndex(x => new
            {
                x.OrganizationId,
                x.ConnectorId,
                x.DedupKey,
            })
            .IsUnique()
            .HasDatabaseName("ux_deduplication_keys_claim");
        builder.HasIndex(x => x.ExpiresAt).HasFilter("expires_at IS NOT NULL");
        builder.UseSnakeCaseColumns();
    }
}
