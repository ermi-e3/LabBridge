using LabBridge.Domain.Features.TestCodeMapping.Entities;
using LabBridge.Domain.Features.TestCodeMapping.Enums;
using LabBridge.Infrastructure.Persistence.Configurations.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LabBridge.Infrastructure.Persistence.Configurations.TestCodeMapping;

/// <summary>
/// One Open/Ignored gap per key. The FRS COALESCE(code,'') / COALESCE(code_system,'') unique index is expressed as four partial unique indexes
/// (code present/absent x code system present/absent).
/// </summary>
public sealed class MappingGapConfiguration : IEntityTypeConfiguration<MappingGap>
{
    public void Configure(EntityTypeBuilder<MappingGap> builder)
    {
        builder.ToTable("mapping_gaps",
            t =>
            {
                t.HasCheckConstraint(
                    "ck_mapping_gaps_resolved",
                    "(status = 'Resolved') = (resolved_at IS NOT NULL AND resolved_by_version_id IS NOT NULL)"
                );
                // One-way on purpose: Resolve from Ignored keeps the ignored reason and actor.
                t.HasCheckConstraint(
                    "ck_mapping_gaps_ignored",
                    "status <> 'Ignored' OR (ignored_reason IS NOT NULL AND ignored_by IS NOT NULL)"
                );
                t.HasCheckConstraint("ck_mapping_gaps_counts", "occurrence_count >= 1 AND last_seen_at >= first_seen_at");
            });
        builder.HasDomainAssignedKey();
        builder.HasTenantAlternateKey();
        builder.Property(x => x.CounterpartyKind).HasEnumAsString(15);
        builder.Property(x => x.Direction).HasEnumAsString(15);
        builder.Property(x => x.Domain).HasEnumAsString(15);
        builder.Property(x => x.GapKind).HasEnumAsString(30);
        builder.Property(x => x.Code).HasMaxLength(100);
        builder.Property(x => x.CodeSystem).HasMaxLength(100);
        builder.Property(x => x.Status).HasEnumAsString(10).HasDefaultValue(GapStatus.Open);
        builder.Property(x => x.OccurrenceCount).IsRequired().HasDefaultValue(1);
        builder.Property(x => x.FirstSeenAt).IsRequired();
        builder.Property(x => x.LastSeenAt).IsRequired();
        builder.Property(x => x.IgnoredReason).HasMaxLength(500);

        builder.HasOrganizationOwner();
        builder
            .HasOne<MappingSet>()
            .WithMany()
            .HasForeignKey(x => new { x.OrganizationId, x.MappingSetId })
            .HasPrincipalKey(s => new { s.OrganizationId, s.Id })
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);
        builder
            .HasOne<MappingSetVersion>()
            .WithMany()
            .HasForeignKey(x => new { x.OrganizationId, x.ResolvedByVersionId })
            .HasPrincipalKey(v => new { v.OrganizationId, v.Id })
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasIndex(x => new
            {
                x.OrganizationId,
                x.CounterpartyId,
                x.Direction,
                x.Domain,
                x.GapKind,
                x.Code,
                x.CodeSystem,
            })
            .IsUnique()
            .HasFilter(
                "status IN ('Open','Ignored') AND code IS NOT NULL AND code_system IS NOT NULL"
            )
            .HasDatabaseName("ux_mapping_gaps_code_system");
        builder
            .HasIndex(x => new
            {
                x.OrganizationId,
                x.CounterpartyId,
                x.Direction,
                x.Domain,
                x.GapKind,
                x.Code,
            })
            .IsUnique()
            .HasFilter("status IN ('Open','Ignored') AND code IS NOT NULL AND code_system IS NULL")
            .HasDatabaseName("ux_mapping_gaps_code");
        builder
            .HasIndex(x => new
            {
                x.OrganizationId,
                x.CounterpartyId,
                x.Direction,
                x.Domain,
                x.GapKind,
                x.CodeSystem,
            })
            .IsUnique()
            .HasFilter("status IN ('Open','Ignored') AND code IS NULL AND code_system IS NOT NULL")
            .HasDatabaseName("ux_mapping_gaps_system");
        builder
            .HasIndex(x => new
            {
                x.OrganizationId,
                x.CounterpartyId,
                x.Direction,
                x.Domain,
                x.GapKind,
            })
            .IsUnique()
            .HasFilter("status IN ('Open','Ignored') AND code IS NULL AND code_system IS NULL")
            .HasDatabaseName("ux_mapping_gaps_none");
        builder.HasIndex(x => new
        {
            x.OrganizationId,
            x.Status,
            x.LastSeenAt,
        });

        builder.IgnoreDomainEvents();
        builder.UseXminConcurrency();
        builder.UseSnakeCaseColumns();
    }
}
