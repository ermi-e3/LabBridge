using LabBridge.Domain.Common.Idempotency;
using LabBridge.Domain.Features.OrganizationManagement.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LabBridge.Infrastructure.Persistence.Configurations.Common;

/// <summary>
/// Shared-kernel table (see A-KERNEL-01). The key is unique per organization (and separately for platform scope, where OrganizationId is null).
/// </summary>
public sealed class IdempotencyRecordConfiguration : IEntityTypeConfiguration<IdempotencyRecord>
{
    public void Configure(EntityTypeBuilder<IdempotencyRecord> builder)
    {
        // added
        builder.ToTable("idempotency_records", t =>
        {
            t.HasCheckConstraint("ck_idempotency_records_key_length", "char_length(key) BETWEEN 16 AND 128");
            t.HasCheckConstraint("ck_idempotency_records_status", "response_status BETWEEN 100 AND 599");
            t.HasCheckConstraint("ck_idempotency_records_expiry", "expires_at > created_at");
        });

        builder.ToTable("idempotency_records");
        builder.HasDomainAssignedKey();
        //builder.Property(x => x.Key).HasMaxLength(128).IsRequired();
        builder.Property(x => x.Key).HasCaseInsensitiveText(128).IsRequired();
        //builder.Property(x => x.Fingerprint).HasMaxLength(128).IsRequired();
        builder.Property(x => x.Fingerprint).HasCaseInsensitiveText(128).IsRequired();
        builder.Property(x => x.ResponseStatus).IsRequired();
        //builder.Property(x => x.ResponseBody).HasColumnType("jsonb");
        builder.Property(x => x.ResponseBody).HasColumnType("text");   // was jsonb: the domain accepts any string
        builder.Property(x => x.CreatedAt).IsRequired();
        builder.Property(x => x.ExpiresAt).IsRequired();

        builder
            .HasOne<Organization>()
            .WithMany()
            .HasForeignKey(x => x.OrganizationId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasIndex(x => new { x.OrganizationId, x.Key })
            .IsUnique()
            .HasFilter("organization_id IS NOT NULL")
            .HasDatabaseName("ux_idempotency_org_key");
        builder
            .HasIndex(x => x.Key)
            .IsUnique()
            .HasFilter("organization_id IS NULL")
            .HasDatabaseName("ux_idempotency_platform_key");
        builder.HasIndex(x => x.ExpiresAt);

        builder.UseSnakeCaseColumns();
    }
}
