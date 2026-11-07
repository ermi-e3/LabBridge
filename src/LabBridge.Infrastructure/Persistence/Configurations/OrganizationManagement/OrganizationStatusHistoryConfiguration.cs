using LabBridge.Domain.Features.OrganizationManagement.Entities;
using LabBridge.Infrastructure.Persistence.Configurations.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LabBridge.Infrastructure.Persistence.Configurations.OrganizationManagement;

/// <summary>Insert-only history. The FK to Organization is configured from the Organization side (aggregate-owned collection).</summary>
public sealed class OrganizationStatusHistoryConfiguration
    : IEntityTypeConfiguration<OrganizationStatusHistory>
{
    public void Configure(EntityTypeBuilder<OrganizationStatusHistory> builder)
    {
        builder.ToTable("organization_status_history",
             t =>
             {
                 t.HasCheckConstraint("ck_organization_status_history_mode", "(to_status = 'Suspended') = (suspension_mode IS NOT NULL)");
                 t.HasCheckConstraint("ck_organization_status_history_change", "from_status IS NULL OR from_status <> to_status");
                 t.HasCheckConstraint("ck_organization_status_history_version", "status_version >= 1");
             });
        builder.HasDomainAssignedKey();
        builder.Property(x => x.OrganizationId).IsRequired();
        builder.Property(x => x.FromStatus).HasEnumAsString(20);
        builder.Property(x => x.ToStatus).HasEnumAsString(20);
        builder.Property(x => x.SuspensionMode).HasEnumAsString(20);
        builder.Property(x => x.Reason).HasMaxLength(500);
        builder.Property(x => x.StatusVersion).IsRequired();
        builder.Property(x => x.ChangedAt).IsRequired();

        builder.HasIndex(x => new { x.OrganizationId, x.StatusVersion }).IsUnique();
        builder.HasIndex(x => new { x.OrganizationId, x.ChangedAt }).IsDescending(false, true);

        builder.UseSnakeCaseColumns();
    }
}
