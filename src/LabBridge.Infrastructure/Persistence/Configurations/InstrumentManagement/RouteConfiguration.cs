using LabBridge.Domain.Features.InstrumentManagement.Entities;
using LabBridge.Infrastructure.Persistence.Configurations.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LabBridge.Infrastructure.Persistence.Configurations.InstrumentManagement;

/// <summary>Route. At most one Enabled route per (organization, source connector, message kind) (BR-CON-009) via a partial unique index.</summary>
public sealed class RouteConfiguration : IEntityTypeConfiguration<Route>
{
    public void Configure(EntityTypeBuilder<Route> builder)
    {
        builder.ToTable("routes", t =>
        {
            t.HasCheckConstraint("ck_routes_endpoints_differ", "source_connector_id <> destination_connector_id");
            t.HasCheckConstraint("ck_routes_message_kind", "message_kind IN ('Order','Result')");
        });

        builder.HasDomainAssignedKey();
        // CHANGE: explicit tenant alternate key (organization_id, id). Route is the principal of composite FKs from
        // DeliveryItem, DeliveryRequest, RouteDeliveryPolicy, Order, Result and MappingRun; the key existed only
        // implicitly through their HasPrincipalKey calls. Same constraint, no schema difference.
        builder.HasTenantAlternateKey();
        builder.Property(x => x.Name).HasCaseInsensitiveText(200).IsRequired();
        builder.Property(x => x.MessageKind).HasEnumAsString(10);
        builder.Property(x => x.Topology).HasEnumAsString(20);
        builder.Property(x => x.Status).HasEnumAsString(10); // no HasDefaultValue: the domain always assigns it
        builder.Property(x => x.IsSimulated).IsRequired();
        builder.Property(x => x.CreatedAt).IsRequired();
        builder.Property(x => x.UpdatedAt).IsRequired();

        builder.HasOrganizationOwner();
        builder
            .HasOne<Connector>()
            .WithMany()
            .HasForeignKey(x => new { x.OrganizationId, x.SourceConnectorId })
            .HasPrincipalKey(c => new { c.OrganizationId, c.Id })
            .OnDelete(DeleteBehavior.Restrict);
        builder
            .HasOne<Connector>()
            .WithMany()
            .HasForeignKey(x => new { x.OrganizationId, x.DestinationConnectorId })
            .HasPrincipalKey(c => new { c.OrganizationId, c.Id })
            .OnDelete(DeleteBehavior.Restrict);
        builder
            .HasOne<Instrument>()
            .WithMany()
            .HasForeignKey(x => new { x.OrganizationId, x.InstrumentId })
            .HasPrincipalKey(i => new { i.OrganizationId, i.Id })
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new { x.OrganizationId, x.Name }).IsUnique();
        builder
            .HasIndex(x => new { x.OrganizationId, x.SourceConnectorId, x.MessageKind })
            .IsUnique()
            .HasFilter("status = 'Enabled'")
            .HasDatabaseName("ux_routes_enabled_source_kind");

        builder.IgnoreDomainEvents();
        builder.UseXminConcurrency();
        builder.UseSnakeCaseColumns();
    }
}