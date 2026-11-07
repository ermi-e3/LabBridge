using LabBridge.Domain.Features.InstrumentManagement.Entities;
using LabBridge.Domain.Features.InstrumentManagement.Enums;
using LabBridge.Domain.Features.TestCodeMapping.Entities;
using LabBridge.Infrastructure.Persistence.Configurations.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LabBridge.Infrastructure.Persistence.Configurations.TestCodeMapping;

/// <summary>
/// One set per (counterparty, direction, domain): the FRS COALESCE(instrument, system) unique index is expressed as two partial unique indexes.
/// ActiveVersionId is a deferrable circular reference in the FRS; it is mapped as a plain FK to the version and must be created DEFERRABLE INITIALLY DEFERRED in the migration.
/// </summary>
public sealed class MappingSetConfiguration : IEntityTypeConfiguration<MappingSet>
{
    public void Configure(EntityTypeBuilder<MappingSet> builder)
    {
        builder.ToTable(
            "mapping_sets", "testCode_mapping",
            t =>
                t.HasCheckConstraint(
                    "ck_mapping_sets_counterparty",
                    "(instrument_id IS NULL) <> (external_system_id IS NULL)"
                )
        );
        builder.HasDomainAssignedKey();
        builder.HasTenantAlternateKey();
        builder.Property(x => x.CounterpartyKind).HasEnumAsString(15);
        builder.Property(x => x.Direction).HasEnumAsString(15);
        builder.Property(x => x.Domain).HasEnumAsString(15);
        builder.Property(x => x.Status).HasEnumAsString(10).HasDefaultValue(ActiveStatus.Active);
        builder.Property(x => x.CreatedAt).IsRequired();
        builder.Property(x => x.UpdatedAt).IsRequired();

        builder.HasOrganizationOwner();
        builder
            .HasOne<Instrument>()
            .WithMany()
            .HasForeignKey(x => new { x.OrganizationId, x.InstrumentId })
            .HasPrincipalKey(i => new { i.OrganizationId, i.Id })
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);
        builder
            .HasOne<ExternalSystem>()
            .WithMany()
            .HasForeignKey(x => new { x.OrganizationId, x.ExternalSystemId })
            .HasPrincipalKey(s => new { s.OrganizationId, s.Id })
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);
        builder
            .HasOne<MappingSetVersion>()
            .WithMany()
            .HasForeignKey(x => new { x.OrganizationId, x.ActiveVersionId })
            .HasPrincipalKey(v => new { v.OrganizationId, v.Id })
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasIndex(x => new
            {
                x.OrganizationId,
                x.InstrumentId,
                x.Direction,
                x.Domain,
            })
            .IsUnique()
            .HasFilter("instrument_id IS NOT NULL")
            .HasDatabaseName("ux_mapping_sets_instrument");
        builder
            .HasIndex(x => new
            {
                x.OrganizationId,
                x.ExternalSystemId,
                x.Direction,
                x.Domain,
            })
            .IsUnique()
            .HasFilter("external_system_id IS NOT NULL")
            .HasDatabaseName("ux_mapping_sets_external_system");

        builder.Ignore(x => x.CounterpartyId);
        builder.IgnoreDomainEvents();
        builder.UseXminConcurrency();
        builder.UseSnakeCaseColumns();
    }
}
