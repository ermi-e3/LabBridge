using LabBridge.Domain.Features.Audit.Entities;
using LabBridge.Domain.Features.Audit.Enums;
using LabBridge.Infrastructure.Persistence.Configurations.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LabBridge.Infrastructure.Persistence.Configurations.Audit;

public sealed class AuditSpoolItemConfiguration : IEntityTypeConfiguration<AuditSpoolItem>
{
    public void Configure(EntityTypeBuilder<AuditSpoolItem> builder)
    {
        builder.ToTable("audit_spool_items", "audit");
        builder.HasDomainAssignedKey();
        builder.UseXminConcurrency();   // a second worker updating the same item fails with DbUpdateConcurrencyException
        builder.Property(x => x.EventJson).HasJsonbColumn().IsRequired();
        builder.Property(x => x.Status).HasEnumAsString(10).HasDefaultValue(SpoolStatus.Pending);
        builder.Property(x => x.AttemptCount).IsRequired().HasDefaultValue(0);
        builder.Property(x => x.NextAttemptAt).IsRequired();
        builder.Property(x => x.CreatedAt).IsRequired();

        builder.HasIndex(x => x.AuditEventId).IsUnique();
        builder.HasIndex(x => new { x.Status, x.NextAttemptAt });
        builder.HasIndex(x => x.NextAttemptAt).HasFilter("status = 'Pending'").HasDatabaseName("ix_audit_spool_items_pending");
        builder.UseSnakeCaseColumns();
    }
}
