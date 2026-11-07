using LabBridge.Domain.Features.InstrumentManagement.Entities;
using LabBridge.Domain.Features.Monitoring.Entities;
using LabBridge.Infrastructure.Persistence.Configurations.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LabBridge.Infrastructure.Persistence.Configurations.Monitoring;

public sealed class ConnectorHealthSnapshotConfiguration
    : IEntityTypeConfiguration<ConnectorHealthSnapshot>
{
    public void Configure(EntityTypeBuilder<ConnectorHealthSnapshot> builder)
    {
        //builder.ToTable("connector_health_snapshots");
        builder.ToTable("connector_health_snapshots", "monitoring", t => t.HasCheckConstraint("ck_connector_health_snapshots_version", "config_version >= 1"));
        builder.HasDomainAssignedKey();
        builder.Property(x => x.Status).HasEnumAsString(15);
        builder.Property(x => x.PreviousStatus).HasEnumAsString(15);
        builder.Property(x => x.LastErrorCode).HasMaxLength(50);
        builder.Property(x => x.ConfigVersion).IsRequired();
        builder.Property(x => x.IsSimulated).IsRequired();
        builder.Property(x => x.OccurredAt).IsRequired();
        builder.Property(x => x.EventKey).HasMaxLength(200).IsRequired();

        builder.HasOrganizationOwner();
        builder
            .HasOne<Connector>()
            .WithMany()
            .HasForeignKey(x => new { x.OrganizationId, x.ConnectorId })
            .HasPrincipalKey(c => new { c.OrganizationId, c.Id })
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new { x.ConnectorId, x.EventKey }).IsUnique();
        builder.HasIndex(x => new { x.ConnectorId, x.OccurredAt }).IsDescending(false, true);

        builder.HasIndex(x => x.OccurredAt).HasDatabaseName("ix_connector_health_snapshots_retention");

        builder.UseSnakeCaseColumns();
    }
}
