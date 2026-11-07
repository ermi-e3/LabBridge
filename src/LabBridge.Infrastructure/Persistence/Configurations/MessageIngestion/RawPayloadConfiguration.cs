using LabBridge.Domain.Features.MessageIngestion.Entities;
using LabBridge.Infrastructure.Persistence.Configurations.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LabBridge.Infrastructure.Persistence.Configurations.MessageIngestion;

/// <summary>Raw content: never indexed, never logged. Purge clears Content only (BR-ING-017).</summary>
public sealed class RawPayloadConfiguration : IEntityTypeConfiguration<RawPayload>
{
    public void Configure(EntityTypeBuilder<RawPayload> builder)
    {
        builder.ToTable(
            "raw_payloads", "message_ingestion",
            t =>
            {
                t.HasCheckConstraint(
                    "ck_raw_payloads_content_or_purged",
                    "(content IS NOT NULL) <> (purged_at IS NOT NULL)"
                );
                t.HasCheckConstraint(
                    "ck_raw_payloads_hash_format",
                    "content_hash ~ '^[0-9a-f]{64}$'"
                );
                // added 
                t.HasCheckConstraint("ck_raw_payloads_size",
                        "size_bytes > 0 AND (content IS NULL OR octet_length(content) = size_bytes)");
            }
        );
        builder.HasDomainAssignedKey();
        builder.HasTenantAlternateKey();
        builder.Property(x => x.Content).HasColumnType("bytea");
        builder.Property(x => x.ContentHash).HasMaxLength(64).IsFixedLength().IsRequired();
        builder.Property(x => x.SizeBytes).IsRequired();
        builder.Property(x => x.EncodingHint).HasMaxLength(50);
        builder.Property(x => x.ProtectionScheme).HasMaxLength(50).IsRequired();
        builder.Property(x => x.KeyRef).HasMaxLength(200);
        builder.Property(x => x.StoredAt).IsRequired();
        builder.Property(x => x.PurgeReason).HasMaxLength(100);

        builder.HasOrganizationOwner();
        builder.HasIndex(x => new { x.OrganizationId, x.StoredAt }); // retention purge
        builder.UseSnakeCaseColumns();
    }
}
