using LabBridge.Domain.Features.InstrumentManagement.Entities;
using LabBridge.Infrastructure.Persistence.Configurations.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LabBridge.Infrastructure.Persistence.Configurations.InstrumentManagement;

/// <summary>Insert-only; produced by Connector.RecordTestResult and persisted without a loaded collection on the aggregate.</summary>
public sealed class ConnectorTestResultConfiguration : IEntityTypeConfiguration<ConnectorTestResult>
{
    public void Configure(EntityTypeBuilder<ConnectorTestResult> builder)
    {
        //builder.ToTable("connector_test_results");
        builder.ToTable("connector_test_results", "instrument_management", t => t.HasCheckConstraint("ck_connector_test_results_times", "completed_at >= started_at"));
        builder.HasDomainAssignedKey();
        builder.Property(x => x.ConfigVersion).IsRequired();
        builder.Property(x => x.Outcome).HasEnumAsString(15);
        builder.Property(x => x.Summary).HasMaxLength(500);
        builder.Property(x => x.ErrorCode).HasMaxLength(50);
        builder.Property(x => x.StartedAt).IsRequired();
        builder.Property(x => x.CompletedAt).IsRequired();

        builder.HasOrganizationOwner();
        builder
            .HasOne<Connector>()
            .WithMany()
            .HasForeignKey(x => new { x.OrganizationId, x.ConnectorId })
            .HasPrincipalKey(c => new { c.OrganizationId, c.Id })
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new { x.ConnectorId, x.StartedAt }).IsDescending(false, true);
        builder.HasIndex(x => new { x.ConnectorId, x.ConfigVersion })
              .HasDatabaseName("ix_connector_test_results_current_config");

        builder.UseSnakeCaseColumns();
    }
}
