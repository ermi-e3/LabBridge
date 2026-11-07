using LabBridge.Domain.Features.DeliveryManagement.Entities;
using LabBridge.Domain.Features.InstrumentManagement.Entities;
using LabBridge.Infrastructure.Persistence.Configurations.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LabBridge.Infrastructure.Persistence.Configurations.DeliveryManagement;

/// <summary>Restricted content; null exactly when purged.</summary>
public sealed class SimulatorSinkMessageConfiguration
    : IEntityTypeConfiguration<SimulatorSinkMessage>
{
    public void Configure(EntityTypeBuilder<SimulatorSinkMessage> builder)
    {
        builder.ToTable(
            "simulator_sink_messages",
            t =>
                t.HasCheckConstraint(
                    "ck_simulator_sink_messages_purged",
                    "(content IS NULL) = (purged_at IS NOT NULL)"
                )
        );
        builder.HasDomainAssignedKey();
        builder.Property(x => x.Content).HasColumnType("bytea");
        builder.Property(x => x.ContentHash).HasMaxLength(64).IsFixedLength().IsRequired();
        builder.Property(x => x.ReceivedAt).IsRequired();

        builder.HasOrganizationOwner();
        builder
            .HasOne<Connector>()
            .WithMany()
            .HasForeignKey(x => new { x.OrganizationId, x.ConnectorId })
            .HasPrincipalKey(c => new { c.OrganizationId, c.Id })
            .OnDelete(DeleteBehavior.Restrict);
        builder
            .HasOne<DeliveryAttempt>()
            .WithMany()
            .HasForeignKey(x => new { x.OrganizationId, x.AttemptId })
            .HasPrincipalKey(a => new { a.OrganizationId, a.Id })
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new { x.ConnectorId, x.ReceivedAt }).IsDescending(false, true);
        builder.UseSnakeCaseColumns();
    }
}
