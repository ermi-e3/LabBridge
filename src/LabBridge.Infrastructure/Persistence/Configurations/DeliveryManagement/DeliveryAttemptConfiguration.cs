using LabBridge.Domain.Features.DeliveryManagement.Entities;
using LabBridge.Infrastructure.Persistence.Configurations.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LabBridge.Infrastructure.Persistence.Configurations.DeliveryManagement;

/// <summary>Append-only. One InProgress attempt per item (partial unique); ControlId unique per organization.</summary>
public sealed class DeliveryAttemptConfiguration : IEntityTypeConfiguration<DeliveryAttempt>
{
    public void Configure(EntityTypeBuilder<DeliveryAttempt> builder)
    {
        builder.ToTable("delivery_attempts", t =>
        {
            t.HasCheckConstraint("ck_delivery_attempts_completed", "(attempt_result = 'InProgress') = (completed_at IS NULL)");
        });
        builder.HasDomainAssignedKey();
        builder.HasTenantAlternateKey();
        builder.Property(x => x.AttemptNumber).IsRequired();
        builder.Property(x => x.ControlId).HasMaxLength(100).IsRequired();
        builder.Property(x => x.TransportOutcome).HasEnumAsString(20);
        builder.Property(x => x.AckOutcome).HasEnumAsString(20);
        builder.Property(x => x.AcceptanceOutcome).HasEnumAsString(20);
        builder.Property(x => x.AttemptResult).HasEnumAsString(20);
        builder.Property(x => x.ErrorCode).HasMaxLength(50);
        builder.Property(x => x.ErrorCategory).HasMaxLength(50);
        builder.Property(x => x.ConnectorConfigVersion).IsRequired();
        builder.Property(x => x.StartedAt).IsRequired();

        builder.HasOrganizationOwner();
        builder.HasIndex(x => new { x.DeliveryId, x.AttemptNumber }).IsUnique();
        builder.HasIndex(x => new { x.OrganizationId, x.ControlId }).IsUnique();
        builder
            .HasIndex(x => x.DeliveryId)
            .IsUnique()
            .HasFilter("attempt_result = 'InProgress'")
            .HasDatabaseName("ux_delivery_attempts_one_in_progress");

        builder.UseSnakeCaseColumns();
    }
}
