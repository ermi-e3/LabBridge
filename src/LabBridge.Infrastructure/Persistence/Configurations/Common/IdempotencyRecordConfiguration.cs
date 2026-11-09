using LabBridge.Domain.Common.Idempotency;
using LabBridge.Domain.Features.OrganizationManagement.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LabBridge.Infrastructure.Persistence.Configurations.Common;

/// <summary>
/// Shared-kernel table (see A-KERNEL-01). The key is unique per organization (and separately for platform scope, where OrganizationId is null).
/// </summary>
//public sealed class IdempotencyRecordConfiguration : IEntityTypeConfiguration<IdempotencyRecord>
//{
//    public void Configure(EntityTypeBuilder<IdempotencyRecord> builder)
//    {
//        // added
//        builder.ToTable("idempotency_records", "common",t =>
//        {
//            t.HasCheckConstraint("ck_idempotency_records_key_length", "char_length(key) BETWEEN 16 AND 128");
//            t.HasCheckConstraint("ck_idempotency_records_status", "response_status BETWEEN 100 AND 599");
//            t.HasCheckConstraint("ck_idempotency_records_expiry", "expires_at > created_at");
//        });

//        //builder.ToTable("idempotency_records");
//        builder.HasDomainAssignedKey();
//        //builder.Property(x => x.Key).HasMaxLength(128).IsRequired();
//        builder.Property(x => x.Key).HasCaseInsensitiveText(128).IsRequired();
//        //builder.Property(x => x.Fingerprint).HasMaxLength(128).IsRequired();
//        builder.Property(x => x.Fingerprint).HasCaseInsensitiveText(128).IsRequired();
//        builder.Property(x => x.ResponseStatus).IsRequired();
//        //builder.Property(x => x.ResponseBody).HasColumnType("jsonb");
//        builder.Property(x => x.ResponseBody).HasColumnType("text");   // was jsonb: the domain accepts any string
//        builder.Property(x => x.CreatedAt).IsRequired();
//        builder.Property(x => x.ExpiresAt).IsRequired();

//        builder
//            .HasOne<Organization>()
//            .WithMany()
//            .HasForeignKey(x => x.OrganizationId)
//            .IsRequired(false)
//            .OnDelete(DeleteBehavior.Restrict);

//        builder
//            .HasIndex(x => new { x.OrganizationId, x.Key })
//            .IsUnique()
//            .HasFilter("organization_id IS NOT NULL")
//            .HasDatabaseName("ux_idempotency_org_key");
//        builder
//            .HasIndex(x => x.Key)
//            .IsUnique()
//            .HasFilter("organization_id IS NULL")
//            .HasDatabaseName("ux_idempotency_platform_key");
//        builder.HasIndex(x => x.ExpiresAt);

//        builder.UseSnakeCaseColumns();
//    }
//}


/// <summary>
/// Configures the shared-kernel idempotency record.
///
/// A key is unique within its organization and operation scope.
/// Platform-scoped records use a separate uniqueness constraint
/// because OrganizationId is null.
/// </summary>
public sealed class IdempotencyRecordConfiguration
    : IEntityTypeConfiguration<IdempotencyRecord>
{
    public void Configure(EntityTypeBuilder<IdempotencyRecord> builder)
    {
        builder.ToTable("idempotency_records", "common", table =>
        {
            table.HasCheckConstraint(
                "ck_idempotency_records_key_length",
                "char_length(key) BETWEEN 16 AND 128");

            table.HasCheckConstraint(
                "ck_idempotency_records_operation_scope",
                "char_length(operation_scope) BETWEEN 1 AND 256");

            table.HasCheckConstraint(
                "ck_idempotency_records_fingerprint_length",
                "char_length(fingerprint) BETWEEN 1 AND 128");

            table.HasCheckConstraint(
                "ck_idempotency_records_status",
                "status IN ('InProgress', 'Completed')");

            table.HasCheckConstraint(
                "ck_idempotency_records_response",
                """
                (
                    status = 'InProgress'
                    AND response_status IS NULL
                )
                OR
                (
                    status = 'Completed'
                    AND response_status BETWEEN 100 AND 599
                )
                """);

            table.HasCheckConstraint(
                "ck_idempotency_records_expiry",
                "expires_at > created_at");
            table.HasCheckConstraint(
    "ck_idempotency_records_reservation_token",
    "reservation_token <> '00000000-0000-0000-0000-000000000000'");
        });

        builder.HasDomainAssignedKey();

        builder.Property(x => x.OrganizationId)
            .IsRequired(false);
        builder.Property(x => x.ReservationToken).IsRequired();

        builder.Property(x => x.OperationScope)
            .HasCaseInsensitiveText(256)
            .IsRequired();

        builder.Property(x => x.Key)
            .HasCaseInsensitiveText(128)
            .IsRequired();

        builder.Property(x => x.Fingerprint)
            .HasMaxLength(128)
            .IsRequired();

        builder.Property(x => x.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(x => x.ResponseStatus)
            .IsRequired(false);

        builder.Property(x => x.ResponseBody)
            .HasColumnType("text")
            .IsRequired(false);

        builder.Property(x => x.ResponseContentType)
            .HasMaxLength(128)
            .IsRequired(false);

        builder.Property(x => x.ResponseHeaders)
            .HasColumnType("jsonb")
            .IsRequired(false);

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.ExpiresAt)
            .IsRequired();

        // Organization relationship.
        builder.HasOne<Organization>()
            .WithMany()
            .HasForeignKey(x => x.OrganizationId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);

        // Organization-scoped uniqueness.
        builder.HasIndex(x => new
        {
            x.OrganizationId,
            x.OperationScope,
            x.Key
        })
            .IsUnique()
            .HasFilter("organization_id IS NOT NULL")
            .HasDatabaseName("ux_idempotency_org_scope_key");

        // Platform-scoped uniqueness.
        builder.HasIndex(x => new
        {
            x.OperationScope,
            x.Key
        })
            .IsUnique()
            .HasFilter("organization_id IS NULL")
            .HasDatabaseName("ux_idempotency_platform_scope_key");

        // Supports expiration cleanup.
        builder.HasIndex(x => x.ExpiresAt)
            .HasDatabaseName("ix_idempotency_expires_at");

        builder.UseSnakeCaseColumns();
    }
}
