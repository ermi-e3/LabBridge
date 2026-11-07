using LabBridge.Domain.Features.MessageIngestion.Entities;
using LabBridge.Domain.Features.MessageProcessing.Entities;
using LabBridge.Infrastructure.Persistence.Configurations.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LabBridge.Infrastructure.Persistence.Configurations.MessageProcessing;

/// <summary>ContentJson is protected clinical data (cleared by purge, never the row). Exactly one current version per message.</summary>
public sealed class NormalizedMessageConfiguration : IEntityTypeConfiguration<NormalizedMessage>
{
    public void Configure(EntityTypeBuilder<NormalizedMessage> builder)
    {
        builder.ToTable(
            "normalized_messages", "message_processing",
            t =>
                {
                    t.HasCheckConstraint(
                    "ck_normalized_messages_purged",
                    "(content_json IS NULL) = (purged_at IS NOT NULL)"
                );
                t.HasCheckConstraint("ck_normalized_messages_version", "version >= 1");

                t.HasCheckConstraint("ck_normalized_messages_content_hash", "content_hash ~ '^[0-9a-f]{64}$'");

                }
        );
        builder.HasDomainAssignedKey();
        builder.HasTenantAlternateKey();
        builder.Property(x => x.Version).IsRequired();
        builder.Property(x => x.IsCurrent).IsRequired();
        builder.Property(x => x.MessageKind).HasEnumAsString(20);
        builder.Property(x => x.CanonicalSchemaVersion).IsRequired();
        builder.Property(x => x.ContentJson).HasJsonbColumn();
        builder.Property(x => x.ContentHash).HasMaxLength(64).IsFixedLength().IsRequired();
        builder.Property(x => x.Consumer).HasEnumAsString(10);
        builder.Property(x => x.CreatedAt).IsRequired();

        builder.HasIndex(x => x.RunId).IsUnique();   // optional: at most one normalized message per run

        builder.HasOrganizationOwner();
        builder
            .HasOne<InboundMessage>()
            .WithMany()
            .HasForeignKey(x => new { x.OrganizationId, x.MessageId })
            .HasPrincipalKey(m => new { m.OrganizationId, m.Id })
            .OnDelete(DeleteBehavior.Restrict);
        //builder
        //    .HasOne<ProcessingRun>()
        //    .WithMany()
        //    .HasForeignKey(x => new { x.OrganizationId, x.RunId })
        //    .HasPrincipalKey(r => new { r.OrganizationId, r.Id })
        //    .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasOne<ProcessingRun>()
            .WithMany()
            .HasForeignKey(x => new { x.OrganizationId, x.MessageId, x.RunId })
            .HasPrincipalKey(r => new { r.OrganizationId, r.MessageId, r.Id })
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new { x.MessageId, x.Version }).IsUnique();
        builder
            .HasIndex(x => x.MessageId)
            .IsUnique()
            .HasFilter("is_current")
            .HasDatabaseName("ux_normalized_messages_current");
        builder.HasIndex(x => new { x.OrganizationId, x.CreatedAt });

        builder.IgnoreDomainEvents();
        builder.UseXminConcurrency();
        builder.UseSnakeCaseColumns();
    }
}
