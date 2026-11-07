using LabBridge.Domain.Features.DeliveryManagement.Entities;
using LabBridge.Domain.Features.DeliveryManagement.Enums;
using LabBridge.Infrastructure.Persistence.Configurations.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LabBridge.Infrastructure.Persistence.Configurations.DeliveryManagement;

/// <summary>One Open case per attempt (partial unique). Resolution is present exactly when Resolved.</summary>
public sealed class ReconciliationCaseConfiguration : IEntityTypeConfiguration<ReconciliationCase>
{
    public void Configure(EntityTypeBuilder<ReconciliationCase> builder)
    {
        //builder.ToTable(
        //    "reconciliation_cases",
        //    t =>
        //        t.HasCheckConstraint(
        //            "ck_reconciliation_cases_resolution",
        //            "(resolution IS NOT NULL) = (status = 'Resolved')"
        //        )
        //);
        builder.ToTable("reconciliation_cases", t =>
        {
            t.HasCheckConstraint("ck_reconciliation_cases_resolution", "(resolution IS NOT NULL) = (status = 'Resolved')");
            t.HasCheckConstraint("ck_reconciliation_cases_resolved_at", "(resolved_at IS NOT NULL) = (status = 'Resolved')");
        });
        builder.HasDomainAssignedKey();
        builder
            .Property(x => x.Status)
            .HasEnumAsString(10)
            .HasDefaultValue(ReconciliationCaseStatus.Open);
        builder.Property(x => x.Resolution).HasEnumAsString(30);
        builder.Property(x => x.ResolutionReason).HasMaxLength(500);
        builder.Property(x => x.EvidenceNote).HasMaxLength(1000);
        builder.Property(x => x.OpenedAt).IsRequired();
        builder.Property(x => x.CreatedAt).IsRequired();
        builder.Property(x => x.UpdatedAt).IsRequired();

        builder.HasOrganizationOwner();
        builder
            .HasOne<DeliveryItem>()
            .WithMany()
            .HasForeignKey(x => new { x.OrganizationId, x.DeliveryId })
            .HasPrincipalKey(i => new { i.OrganizationId, i.Id })
            .OnDelete(DeleteBehavior.Restrict);
        builder
            .HasOne<DeliveryAttempt>()
            .WithMany()
            .HasForeignKey(x => new { x.OrganizationId, x.AttemptId })
            .HasPrincipalKey(a => new { a.OrganizationId, a.Id })
            .OnDelete(DeleteBehavior.Restrict);

        //builder
        //    .HasIndex(x => x.AttemptId)
        //    .IsUnique()
        //    .HasFilter("status = 'Open'")
        //    .HasDatabaseName("ux_reconciliation_cases_open_attempt");
        builder.HasIndex(x => x.AttemptId).IsUnique()
            .HasFilter("status IN ('Open','Escalated')")
            .HasDatabaseName("ux_reconciliation_cases_open_attempt");

        builder.HasIndex(x => new
        {
            x.OrganizationId,
            x.Status,
            x.OpenedAt,
        });

        builder.IgnoreDomainEvents();
        builder.UseXminConcurrency();
        builder.UseSnakeCaseColumns();
    }
}
