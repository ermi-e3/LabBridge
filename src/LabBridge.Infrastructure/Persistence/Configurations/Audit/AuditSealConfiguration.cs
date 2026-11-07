using LabBridge.Domain.Features.Audit.Entities;
using LabBridge.Domain.Features.OrganizationManagement.Entities;
using LabBridge.Infrastructure.Persistence.Configurations.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LabBridge.Infrastructure.Persistence.Configurations.Audit;

public sealed class AuditSealConfiguration : IEntityTypeConfiguration<AuditSeal>
{
    public void Configure(EntityTypeBuilder<AuditSeal> builder)
    {
        builder.ToTable("audit_seals", "audit");
        builder.HasDomainAssignedKey();
        builder.Property(x => x.SealSequence).IsRequired();
        builder.Property(x => x.FromEventSequence).IsRequired();
        builder.Property(x => x.ToEventSequence).IsRequired();
        builder.Property(x => x.EventCount).IsRequired();
        //builder.Property(x => x.BlockHash).HasMaxLength(64).IsRequired();
        builder.Property(x => x.BlockHash).HasMaxLength(64).IsFixedLength().IsRequired();
        //builder.Property(x => x.PreviousBlockHash).HasMaxLength(64);
        builder.Property(x => x.PreviousBlockHash).HasMaxLength(64).IsFixedLength();
        builder.Property(x => x.SealedAt).IsRequired();

        builder
            .HasOne<Organization>()
            .WithMany()
            .HasForeignKey(x => x.OrganizationId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasIndex(x => new { x.OrganizationId, x.SealSequence })
            .IsUnique()
            .HasFilter("organization_id IS NOT NULL")
            .HasDatabaseName("ux_audit_seals_org_sequence");
        builder
            .HasIndex(x => x.SealSequence)
            .IsUnique()
            .HasFilter("organization_id IS NULL")
            .HasDatabaseName("ux_audit_seals_platform_sequence");

        builder.ToTable(t =>
        {
            t.HasCheckConstraint("ck_audit_seals_sequence", "seal_sequence >= 1");
            t.HasCheckConstraint("ck_audit_seals_range", "to_event_sequence >= from_event_sequence AND event_count >= 1");
            t.HasCheckConstraint("ck_audit_seals_block_hash", "block_hash ~ '^[0-9a-f]{64}$'");
            t.HasCheckConstraint("ck_audit_seals_chain", "(previous_block_hash IS NULL) = (seal_sequence = 1)");
        });

        builder.UseSnakeCaseColumns();


    }
}
