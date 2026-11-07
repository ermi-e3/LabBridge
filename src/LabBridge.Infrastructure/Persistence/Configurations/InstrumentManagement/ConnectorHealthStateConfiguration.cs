using LabBridge.Domain.Features.InstrumentManagement.Entities;
using LabBridge.Domain.Features.InstrumentManagement.Enums;
using LabBridge.Infrastructure.Persistence.Configurations.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LabBridge.Infrastructure.Persistence.Configurations.InstrumentManagement;

/// <summary>
/// 1:1 with Connector, sharing the connector identifier as primary key. Written only by the runtime (single owner per connector).
/// Deliberately has no xmin concurrency token: last-write-wins with monotonic UpdatedAt (FRS DB-CON-007).
/// </summary>
public sealed class ConnectorHealthStateConfiguration
    : IEntityTypeConfiguration<ConnectorHealthState>
{
    public void Configure(EntityTypeBuilder<ConnectorHealthState> builder)
    {
        builder.ToTable("connector_health_states", "instrument_management");
        builder.HasDomainAssignedKey();
        builder
            .Property(x => x.Status)
            .HasEnumAsString(15)
            .HasDefaultValue(ConnectorHealthStatus.Unknown);
        builder.Property(x => x.ConsecutiveFailures).IsRequired().HasDefaultValue(0);
        builder.Property(x => x.LastErrorCode).HasMaxLength(50);
        builder.Property(x => x.LastErrorSummary).HasMaxLength(500);
        builder.Property(x => x.StatusChangedAt).IsRequired();
        builder.Property(x => x.UpdatedAt).IsRequired();

        builder.HasOrganizationOwner();
        builder
            .HasOne<Connector>()
            .WithOne()
            .HasForeignKey<ConnectorHealthState>(x => new { x.OrganizationId, x.Id })
            .HasPrincipalKey<Connector>(c => new { c.OrganizationId, c.Id })
            .OnDelete(DeleteBehavior.Restrict);

        builder.Ignore(x => x.ConnectorId);
        builder.IgnoreDomainEvents();
        builder.UseSnakeCaseColumns();
    }
}
