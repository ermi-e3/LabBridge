using LabBridge.Domain.Features.DeliveryManagement.Entities;
using LabBridge.Domain.Features.InstrumentManagement.Entities;
using LabBridge.Infrastructure.Persistence.Configurations.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LabBridge.Infrastructure.Persistence.Configurations.DeliveryManagement;

/// <summary>Primary key is the route identifier (also a foreign key to Route).</summary>
public sealed class RouteDeliveryPolicyConfiguration : IEntityTypeConfiguration<RouteDeliveryPolicy>
{
    public void Configure(EntityTypeBuilder<RouteDeliveryPolicy> builder)
    {
        builder.ToTable(
            "route_delivery_policies",
            t =>
            {
                t.HasCheckConstraint("ck_route_delivery_policies_attempts", "max_attempts >= 1");
                t.HasCheckConstraint(
                    "ck_route_delivery_policies_delays",
                    "base_delay_seconds >= 0 AND max_delay_seconds >= base_delay_seconds"
                );
                t.HasCheckConstraint(
                    "ck_route_delivery_policies_timeouts",
                    "send_timeout_seconds > 0 AND ack_timeout_seconds > 0"
                );
                t.HasCheckConstraint(
                    "ck_route_delivery_policies_concurrency",
                    "max_concurrency >= 1"
                );
                t.HasCheckConstraint("ck_route_delivery_policies_escalation", "reconciliation_escalation_minutes > 0");
                t.HasCheckConstraint("ck_route_delivery_policies_circuit",
                    "(circuit_failure_threshold IS NULL) = (circuit_pause_seconds IS NULL) AND COALESCE(circuit_failure_threshold, 1) >= 1 AND COALESCE(circuit_pause_seconds, 1) > 0");
            }
        );
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever().HasColumnName("route_id");
        builder.Property(x => x.Jitter).HasEnumAsString(10);
        builder.Property(x => x.UpdatedAt).IsRequired();

        builder.HasOrganizationOwner();
        builder
            .HasOne<Route>()
            .WithOne()
            .HasForeignKey<RouteDeliveryPolicy>(x => new { x.OrganizationId, x.Id })
            .HasPrincipalKey<Route>(r => new { r.OrganizationId, r.Id })
            .OnDelete(DeleteBehavior.Restrict);

        builder.Ignore(x => x.RouteId);
        builder.IgnoreDomainEvents();
        builder.UseXminConcurrency();
        builder.UseSnakeCaseColumns();
    }
}
