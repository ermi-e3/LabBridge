using LabBridge.Domain.Features.Audit.Entities;
using LabBridge.Domain.Features.OrganizationManagement.Entities;
using LabBridge.Infrastructure.Persistence.Configurations.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LabBridge.Infrastructure.Persistence.Configurations.Audit;

/// <summary>
/// Append-only. The application database role has INSERT/SELECT only and triggers reject UPDATE/DELETE except the one-time sealing columns
/// (stream_sequence, seal_id): both are migration-level controls that this mapping cannot express. Stream sequence uniqueness is expressed per stream.
/// </summary>
public sealed class AuditEventConfiguration : IEntityTypeConfiguration<AuditEvent>
{
    public void Configure(EntityTypeBuilder<AuditEvent> builder)
    {
        //builder.ToTable(
        //    "audit_events",
        //    t =>
        //        t.HasCheckConstraint(
        //            "ck_audit_events_scope",
        //            "(scope = 'Platform') = (organization_id IS NULL)"
        //        )
        //);

        builder.ToTable("audit_events", "audit", tb =>
        {
            tb.HasCheckConstraint("ck_audit_events_scope", "(scope = 'Platform') = (organization_id IS NULL)");
            tb.HasCheckConstraint("ck_audit_events_content_hash", "content_hash ~ '^[0-9a-f]{64}$'");
            tb.HasCheckConstraint("ck_audit_events_schema_version", "schema_version >= 1");
            tb.HasCheckConstraint("ck_audit_events_sealing", "(stream_sequence IS NULL) = (seal_id IS NULL)");
        });

        //builder.ToTable(tb =>
        //    {
        //        tb.HasCheckConstraint("ck_audit_events_scope", "(scope = 'Platform') = (organization_id IS NULL)");
        //        tb.HasCheckConstraint("ck_audit_events_content_hash", "content_hash ~ '^[0-9a-f]{64}$'");
        //        tb.HasCheckConstraint("ck_audit_events_schema_version", "schema_version >= 1");
        //        tb.HasCheckConstraint("ck_audit_events_sealing", "(stream_sequence IS NULL) = (seal_id IS NULL)");
        //    });

        builder.HasDomainAssignedKey();
        builder.Property(x => x.ActionCode).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Scope).HasEnumAsString(15);
        builder.Property(x => x.OccurredAt).IsRequired();
        builder.Property(x => x.RecordedAt).IsRequired();
        builder.HasIndex(x => x.RecordedAt).HasFilter("seal_id IS NULL").HasDatabaseName("ix_audit_events_unsealed");
        builder.Property(x => x.ActorType).HasEnumAsString(15);
        builder.Property(x => x.ActorId).HasMaxLength(100);
        builder.Property(x => x.TargetType).HasMaxLength(50);
        builder.Property(x => x.TargetId).HasMaxLength(100);
        builder.Property(x => x.Outcome).HasEnumAsString(10);
        builder.Property(x => x.TraceId).HasMaxLength(64);
        builder.Property(x => x.RequestId).HasMaxLength(100);
        //builder.Property(x => x.RelatedIdsJson).HasJsonbColumn();
        // json (not jsonb): the exact text participates in ContentHash, jsonb would rewrite it
        builder.Property(x => x.RelatedIdsJson).HasColumnType("json");
        builder.Property(x => x.SourceAddress).HasMaxLength(64);
        builder.Property(x => x.Reason).HasMaxLength(500);
        //builder.Property(x => x.DetailJson).HasJsonbColumn();
        builder.Property(x => x.DetailJson).HasColumnType("json");
        builder.Property(x => x.IsSimulated).IsRequired();
        builder.Property(x => x.TimeSkewFlag).IsRequired().HasDefaultValue(false);
        builder.Property(x => x.SchemaVersion).IsRequired();
        builder.Property(x => x.ContentHash).HasMaxLength(64).IsFixedLength().IsRequired();

        builder
            .HasOne<Organization>()
            .WithMany()
            .HasForeignKey(x => x.OrganizationId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);
        builder
            .HasOne<AuditActionDefinition>()
            .WithMany()
            .HasForeignKey(x => x.ActionCode)
            .OnDelete(DeleteBehavior.Restrict);
        builder
            .HasOne<AuditSeal>()
            .WithMany()
            .HasForeignKey(x => x.SealId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasIndex(x => new { x.OrganizationId, x.StreamSequence })
            .IsUnique()
            .HasFilter("stream_sequence IS NOT NULL AND organization_id IS NOT NULL")
            .HasDatabaseName("ux_audit_events_org_sequence");
        builder
            .HasIndex(x => x.StreamSequence)
            .IsUnique()
            .HasFilter("stream_sequence IS NOT NULL AND organization_id IS NULL")
            .HasDatabaseName("ux_audit_events_platform_sequence");
        builder.HasIndex(x => new { x.OrganizationId, x.OccurredAt });
        builder.HasIndex(x => new { x.OrganizationId, x.CorrelationId });
        builder.HasIndex(x => new
        {
            x.OrganizationId,
            x.TargetType,
            x.TargetId,
        });
        builder.HasIndex(x => new
        {
            x.OrganizationId,
            x.ActionCode,
            x.OccurredAt,
        });
        // GIN needs jsonb: index a stored cast instead (verify the cast is accepted in a generated column in your PostgreSQL version)
        builder.Property<string?>("RelatedIdsIndexed")
            .HasColumnName("related_ids_indexed")
            .HasColumnType("jsonb")
            .HasComputedColumnSql("related_ids_json::jsonb", stored: true);

        builder.HasIndex("RelatedIdsIndexed").HasMethod("gin");   // replaces HasIndex(x => x.RelatedIdsJson).HasMethod("gin")
        //builder.HasIndex(x => x.RelatedIdsJson).HasMethod("gin");

        builder.UseSnakeCaseColumns();
    }
}
