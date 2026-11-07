using LabBridge.Domain.Features.InstrumentManagement.Entities;
using LabBridge.Domain.Features.TestCodeMapping.Entities;
using LabBridge.Infrastructure.Persistence.Configurations.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LabBridge.Infrastructure.Persistence.Configurations.TestCodeMapping;

/// <summary>Immutable execution record. (OrganizationId, RequestId) provides idempotency.</summary>
public sealed class MappingRunConfiguration : IEntityTypeConfiguration<MappingRun>
{
    public void Configure(EntityTypeBuilder<MappingRun> builder)
    {
        builder.ToTable("mapping_runs",
            t =>
            {
                t.HasCheckConstraint("ck_mapping_runs_counts", "resolution_count >= 0 AND failure_count >= 0 AND failure_count <= resolution_count");
                t.HasCheckConstraint("ck_mapping_runs_times", "completed_at >= started_at");
                t.HasCheckConstraint("ck_mapping_runs_status", "(status = 'Failed') = (failure_count > 0)");
            });
        builder.HasDomainAssignedKey();
        builder.HasTenantAlternateKey();
        builder.Property(x => x.SubjectType).HasEnumAsString(10);
        builder.Property(x => x.Trigger).HasEnumAsString(15);
        builder.Property(x => x.Status).HasEnumAsString(10);
        builder.Property(x => x.TraceId).HasMaxLength(64);
        builder.Property(x => x.StartedAt).IsRequired();
        builder.Property(x => x.CompletedAt).IsRequired();

        builder.HasOrganizationOwner();
        builder
            .HasOne<Route>()
            .WithMany()
            .HasForeignKey(x => new { x.OrganizationId, x.RouteId })
            .HasPrincipalKey(r => new { r.OrganizationId, r.Id })
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new { x.OrganizationId, x.RequestId }).IsUnique();
        builder.HasIndex(x => new
        {
            x.OrganizationId,
            x.SubjectType,
            x.SubjectId,
        });

        builder.IgnoreDomainEvents();
        builder.UseSnakeCaseColumns();
    }
}
