using LabBridge.Domain.Features.Reporting.Entities;
using LabBridge.Infrastructure.Persistence.Configurations.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LabBridge.Infrastructure.Persistence.Configurations.Reporting;

/// <summary>Result holds aggregates only. (OrganizationId, RequestKey) is unique when a key is supplied.</summary>
public sealed class ReportRunConfiguration : IEntityTypeConfiguration<ReportRun>
{
    public void Configure(EntityTypeBuilder<ReportRun> builder)
    {
        builder.ToTable("report_runs", "reporting",
            t =>
            {
                // The result exists exactly for a successful run; it is cleared on expiry.
                t.HasCheckConstraint("ck_report_runs_result", "(status = 'Succeeded') = (result_json IS NOT NULL)");
                t.HasCheckConstraint("ck_report_runs_completed", "(status = 'Running') = (completed_at IS NULL)");
                t.HasCheckConstraint("ck_report_runs_expiry", "status <> 'Succeeded' OR expires_at IS NOT NULL");
                t.HasCheckConstraint("ck_report_runs_partial", "NOT partial OR status IN ('Succeeded','Expired')");
            });
        builder.HasDomainAssignedKey();
        builder.Property(x => x.ReportCode).HasMaxLength(100).IsRequired();
        builder.Property(x => x.ParametersJson).HasJsonbColumn().IsRequired();
        builder.Property(x => x.Status).HasEnumAsString(10);
        builder.Property(x => x.IncludesSimulated).IsRequired();
        builder.Property(x => x.Partial).IsRequired().HasDefaultValue(false);
        //builder.Property(x => x.ResultJson).has();
        builder.Property(x => x.ResultJson).HasJsonbColumn();
        builder.Property(x => x.RequestKey).HasMaxLength(100);
        builder.Property(x => x.RequestedBy).IsRequired();
        builder.Property(x => x.CreatedAt).IsRequired();

        builder.HasOrganizationOwner();
        builder
            .HasOne<ReportDefinition>()
            .WithMany()
            .HasForeignKey(x => x.ReportCode)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasIndex(x => new { x.OrganizationId, x.RequestKey })
            .IsUnique()
            .HasFilter("request_key IS NOT NULL");
        builder.HasIndex(x => new { x.OrganizationId, x.CreatedAt }).IsDescending(false, true);
        builder.HasIndex(x => x.ExpiresAt);

        // Retention purge only looks at successful runs that have an expiry.
        builder
            .HasIndex(x => x.ExpiresAt)
            .HasFilter("status = 'Succeeded'")
            .HasDatabaseName("ix_report_runs_expiry");

        builder.IgnoreDomainEvents();
        builder.UseXminConcurrency();   // ReportRun is a mutable aggregate root; without a token two workers can both complete the same run
        builder.UseSnakeCaseColumns();
    }
}
