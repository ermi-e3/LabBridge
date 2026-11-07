using LabBridge.Domain.Features.InstrumentManagement.Entities;
using LabBridge.Infrastructure.Persistence.Configurations.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LabBridge.Infrastructure.Persistence.Configurations.InstrumentManagement;

/// <summary>Immutable configuration snapshots; (ConnectorId, ConfigVersion) is unique. Insert-only.</summary>
public sealed class ConnectorConfigRevisionConfiguration
    : IEntityTypeConfiguration<ConnectorConfigRevision>
{
    public void Configure(EntityTypeBuilder<ConnectorConfigRevision> builder)
    {
        builder.ToTable("connector_config_revisions");
        builder.HasDomainAssignedKey();
        builder.Property(x => x.ConfigVersion).IsRequired();
        builder.Property(x => x.SnapshotJson).HasJsonbColumn().IsRequired();
        builder.Property(x => x.ChangedAt).IsRequired();
        builder.Property(x => x.ChangeReason).HasMaxLength(500);

        builder.HasOrganizationOwner();
        builder
            .HasOne<Connector>()
            .WithMany()
            .HasForeignKey(x => new { x.OrganizationId, x.ConnectorId })
            .HasPrincipalKey(c => new { c.OrganizationId, c.Id })
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new { x.ConnectorId, x.ConfigVersion }).IsUnique();

        builder.UseSnakeCaseColumns();
    }
}
