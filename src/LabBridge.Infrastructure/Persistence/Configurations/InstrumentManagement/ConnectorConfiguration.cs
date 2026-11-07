using LabBridge.Domain.Features.InstrumentManagement.Entities;
using LabBridge.Domain.Features.InstrumentManagement.Enums;
using LabBridge.Infrastructure.Persistence.Configurations.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LabBridge.Infrastructure.Persistence.Configurations.InstrumentManagement;

/// <summary>
/// Connector. Exactly one counterparty (instrument XOR external system) through composite tenant FKs.
/// BindingKeyHash is unique platform-wide (partial, not null): documented exception to the tenant-scoped unique convention (R-CON-4, BR-CON-018).
/// </summary>
public sealed class ConnectorConfiguration : IEntityTypeConfiguration<Connector>
{
    public void Configure(EntityTypeBuilder<Connector> builder)
    {
        builder.ToTable(
            "connectors",
            t =>
            {
                t.HasCheckConstraint(
                    "ck_connectors_single_counterparty",
                    "(instrument_id IS NULL) <> (external_system_id IS NULL)"
                );
                t.HasCheckConstraint(
                    "ck_connectors_binding_required",
                    "direction = 'Outbound' OR binding_key_hash IS NOT NULL"
                );
            }
        );

        builder.HasDomainAssignedKey();
        builder.HasTenantAlternateKey();
        builder.Property(x => x.Name).HasCaseInsensitiveText(200).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(1000);
        builder.Property(x => x.Direction).HasEnumAsString(15);
        builder.Property(x => x.IsSimulated).IsRequired();
        builder.Property(x => x.SettingsJson).HasJsonbColumn().IsRequired();
        builder.Property(x => x.ConfigVersion).IsRequired().HasDefaultValue(1);
        builder.Property(x => x.Status).HasEnumAsString(10).HasDefaultValue(ConnectorStatus.Draft);
        builder.Property(x => x.BindingKeyHash).HasColumnType("bytea");
        builder.Property(x => x.EverEnabled).IsRequired().HasDefaultValue(false);
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
            .HasOne<AdapterType>()
            .WithMany()
            .HasForeignKey(x => x.AdapterTypeId)
            .OnDelete(DeleteBehavior.Restrict);
        builder
            .HasOne<InterfaceProfileVersion>()
            .WithMany()
            .HasForeignKey(x => x.ProfileVersionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasMany(x => x.SecretRefs)
            .WithOne()
            .HasForeignKey(s => new { s.OrganizationId, s.ConnectorId })
            .HasPrincipalKey(c => new { c.OrganizationId, c.Id })
            .OnDelete(DeleteBehavior.Restrict);
        //builder
        //    .Navigation(x => x.SecretRefs)
        //    .HasField("_secretRefs")
        //    .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Navigation(x => x.SecretRefs)
            .HasField("_secretRefs")
            .UsePropertyAccessMode(PropertyAccessMode.Field)
            .AutoInclude();                       // few slots per connector; the revision snapshot depends on them

        builder.HasIndex(x => new { x.OrganizationId, x.Name }).IsUnique();


        builder.HasIndex(x => x.BindingKeyHash).IsUnique().HasDatabaseName("ux_connectors_binding_key_hash");

        //builder
        //    .HasIndex(x => x.BindingKeyHash)
        //    .IsUnique()
        //    .HasFilter("binding_key_hash IS NOT NULL")
        //    .HasDatabaseName("ux_connectors_binding_key_hash");

        builder.HasIndex(x => new { x.OrganizationId, x.Status });
        builder.HasIndex(x => new { x.OrganizationId, x.InstrumentId });
        builder.HasIndex(x => new { x.OrganizationId, x.ExternalSystemId });

        builder.Ignore(x => x.CanReceive);
        builder.Ignore(x => x.CanSend);
        builder.IgnoreDomainEvents();
        builder.UseXminConcurrency();
        builder.UseSnakeCaseColumns();
    }
}
