using LabBridge.Domain.Features.Audit.Entities;
using LabBridge.Domain.Features.OrganizationManagement.Entities;
using LabBridge.Infrastructure.Persistence.Configurations.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LabBridge.Infrastructure.Persistence.Configurations.Audit;

public sealed class AuditIntegrityCheckConfiguration : IEntityTypeConfiguration<AuditIntegrityCheck>
{
    public void Configure(EntityTypeBuilder<AuditIntegrityCheck> builder)
    {
        builder.ToTable("audit_integrity_checks");
        builder.HasDomainAssignedKey();
        builder.Property(x => x.Stream).HasEnumAsString(15);
        builder.Property(x => x.RangeFrom).IsRequired();
        builder.Property(x => x.RangeTo).IsRequired();
        builder.Property(x => x.Status).HasEnumAsString(10);
        //builder.Property(x => x.FirstBreakJson).HasJsonbColumn();
        builder.Property(x => x.FirstBreakJson).HasColumnType("json");

        builder.Property(x => x.RequestedBy).IsRequired();
        builder.Property(x => x.StartedAt).IsRequired();

        builder.ToTable("audit_integrity_checks", t =>
        {
            t.HasCheckConstraint("ck_audit_integrity_checks_stream", "(stream = 'Platform') = (organization_id IS NULL)");
            t.HasCheckConstraint("ck_audit_integrity_checks_range", "range_to > range_from");
            t.HasCheckConstraint("ck_audit_integrity_checks_completed", "(status = 'Running') = (completed_at IS NULL)");
        });

        builder
            .HasOne<Organization>()
            .WithMany()
            .HasForeignKey(x => x.OrganizationId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(x => new { x.OrganizationId, x.StartedAt }).IsDescending(false, true);
        builder.UseSnakeCaseColumns();
    }
}
