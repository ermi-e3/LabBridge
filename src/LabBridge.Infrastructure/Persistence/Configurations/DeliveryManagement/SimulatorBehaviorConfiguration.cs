using LabBridge.Domain.Features.DeliveryManagement.Entities;
using LabBridge.Domain.Features.InstrumentManagement.Entities;
using LabBridge.Infrastructure.Persistence.Configurations.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LabBridge.Infrastructure.Persistence.Configurations.DeliveryManagement;

/// <summary>Primary key is the connector identifier (also a foreign key). Eligibility (simulated simulator-outbound connector) is a domain/application guard.</summary>
public sealed class SimulatorBehaviorConfiguration : IEntityTypeConfiguration<SimulatorBehavior>
{
    public void Configure(EntityTypeBuilder<SimulatorBehavior> builder)
    {
        builder.ToTable("simulator_behaviors", "delivery_management", t =>
        {
            t.HasCheckConstraint("ck_simulator_behaviors_remaining", "remaining_count >= 0");
            t.HasCheckConstraint("ck_simulator_behaviors_delay", "delay_seconds BETWEEN 0 AND 3600");
        });
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever().HasColumnName("connector_id");
        builder.Property(x => x.Mode).HasEnumAsString(20);
        builder.Property(x => x.RemainingCount).IsRequired();
        builder.Property(x => x.DelaySeconds).IsRequired().HasDefaultValue(0);
        builder.Property(x => x.UpdatedAt).IsRequired();

        builder.HasOrganizationOwner();
        builder
            .HasOne<Connector>()
            .WithOne()
            .HasForeignKey<SimulatorBehavior>(x => new { x.OrganizationId, x.Id })
            .HasPrincipalKey<Connector>(c => new { c.OrganizationId, c.Id })
            .OnDelete(DeleteBehavior.Restrict);

        builder.Ignore(x => x.ConnectorId);
        builder.IgnoreDomainEvents();
        builder.UseXminConcurrency();   // or consume with an atomic UPDATE ... SET remaining_count = remaining_count - 1 ... RETURNING
        builder.UseSnakeCaseColumns();
    }
}
