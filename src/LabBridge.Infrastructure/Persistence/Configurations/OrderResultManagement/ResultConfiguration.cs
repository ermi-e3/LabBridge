using LabBridge.Domain.Features.InstrumentManagement.Entities;
using LabBridge.Domain.Features.MessageIngestion.Entities;
using LabBridge.Domain.Features.MessageProcessing.Entities;
using LabBridge.Domain.Features.OrderResultManagement.Entities;
using LabBridge.Domain.Features.OrderResultManagement.Enums;
using LabBridge.Domain.Features.TestCodeMapping.Entities;
using LabBridge.Infrastructure.Persistence.Configurations.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LabBridge.Infrastructure.Persistence.Configurations.OrderResultManagement;

/// <summary>Physical table lab_result. Enriched fields persisted as text[] from the private backing list.</summary>
public sealed class ResultConfiguration : IEntityTypeConfiguration<Result>
{
    public void Configure(EntityTypeBuilder<Result> builder)
    {
        //builder.ToTable("lab_result");
        builder.ToTable(
                    "lab_result", "orderResult_management",
                    t =>
                    {
                        t.HasCheckConstraint("ck_lab_result_correlation", "(correlation_status = 'Matched') = (order_id IS NOT NULL)");
                        t.HasCheckConstraint(
                            "ck_lab_result_correlation_evidence",
                            "(order_id IS NOT NULL) = (correlation_method IS NOT NULL AND correlated_at IS NOT NULL)"
                        );
                        t.HasCheckConstraint(
                            "ck_lab_result_quarantine",
                            "status <> 'Quarantined' OR (quarantine_reason IS NOT NULL AND quarantined_from_status IS NOT NULL)"
                        );
                        t.HasCheckConstraint("ck_lab_result_content_hash", "content_hash ~ '^[0-9a-f]{64}$'");
                        t.HasCheckConstraint("ck_lab_result_payload_version", "payload_version >= 1");
                    }
                );

        builder.HasDomainAssignedKey();
        builder.HasTenantAlternateKey();
        builder.Property(x => x.CorrelationId).IsRequired();
        builder
            .Property(x => x.CorrelationStatus)
            .HasEnumAsString(15);
            //.HasDefaultValue(CorrelationStatus.Unmatched); // no HasDefaultValue: the domain always assigns it
        builder.Property(x => x.CorrelationMethod).HasEnumAsString(15);
        builder.Property(x => x.CorrelationBasis).HasMaxLength(100);
        builder.Property(x => x.PlacerOrderId).HasMaxLength(100);
        builder.Property(x => x.FillerOrderId).HasMaxLength(100);
        builder.Property(x => x.AccessionNumber).HasMaxLength(100);
        builder.Property(x => x.SpecimenId).HasMaxLength(100);
        builder.Property(x => x.MessageControlId).HasMaxLength(100);
        builder.Property(x => x.Status).HasEnumAsString(20).HasDefaultValue(ResultStatus.Received);
        builder.Property(x => x.StatusReason).HasMaxLength(500);
        builder.Property(x => x.QuarantineReason).HasEnumAsString(30);
        builder.Property(x => x.QuarantinedFromStatus).HasEnumAsString(20);
        builder.Property(x => x.PossibleCorrection).IsRequired().HasDefaultValue(false);
        builder.Property(x => x.ContentHash).HasMaxLength(64).IsRequired();
        builder.Property(x => x.PayloadVersion).IsRequired().HasDefaultValue(1);
        builder.Property(x => x.IsSimulated).IsRequired();
        builder.Property(x => x.CreatedAt).IsRequired();
        builder.Property(x => x.UpdatedAt).IsRequired();

        builder.Ignore(x => x.EnrichedFields);
        builder
            .Property<List<string>>("_enrichedFields")
            .HasColumnName("enriched_fields")
            .HasColumnType("text[]")
            .IsRequired();

        builder.HasOrganizationOwner();
        builder
            .HasOne<Connector>()
            .WithMany()
            .HasForeignKey(x => new { x.OrganizationId, x.SourceConnectorId })
            .HasPrincipalKey(c => new { c.OrganizationId, c.Id })
            .OnDelete(DeleteBehavior.Restrict);
        builder
            .HasOne<Instrument>()
            .WithMany()
            .HasForeignKey(x => new { x.OrganizationId, x.SourceInstrumentId })
            .HasPrincipalKey(i => new { i.OrganizationId, i.Id })
            .OnDelete(DeleteBehavior.Restrict);
        builder
            .HasOne<Route>()
            .WithMany()
            .HasForeignKey(x => new { x.OrganizationId, x.RouteId })
            .HasPrincipalKey(r => new { r.OrganizationId, r.Id })
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);
        builder
            .HasOne<ExternalSystem>()
            .WithMany()
            .HasForeignKey(x => new { x.OrganizationId, x.DestinationExternalSystemId })
            .HasPrincipalKey(s => new { s.OrganizationId, s.Id })
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);
        builder
            .HasOne<Order>()
            .WithMany()
            .HasForeignKey(x => new { x.OrganizationId, x.OrderId })
            .HasPrincipalKey(o => new { o.OrganizationId, o.Id })
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);
        builder
            .HasOne<InboundMessage>()
            .WithMany()
            .HasForeignKey(x => new { x.OrganizationId, x.SourceMessageId })
            .HasPrincipalKey(m => new { m.OrganizationId, m.Id })
            .OnDelete(DeleteBehavior.Restrict);
        builder
            .HasOne<NormalizedMessage>()
            .WithMany()
            .HasForeignKey(x => new { x.OrganizationId, x.NormalizedMessageId })
            .HasPrincipalKey(m => new { m.OrganizationId, m.Id })
            .OnDelete(DeleteBehavior.Restrict);
        builder
            .HasOne<MappingRun>()
            .WithMany()
            .HasForeignKey(x => new { x.OrganizationId, x.MappingRunId })
            .HasPrincipalKey(m => new { m.OrganizationId, m.Id })
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasMany(x => x.Observations)
            .WithOne()
            .HasForeignKey(o => new { o.OrganizationId, o.ResultId })
            .HasPrincipalKey(r => new { r.OrganizationId, r.Id })
            .OnDelete(DeleteBehavior.Restrict);
        builder
            .Navigation(x => x.Observations)
            .HasField("_observations")
            .UsePropertyAccessMode(PropertyAccessMode.Field);
        builder
            .HasMany(x => x.StatusHistory)
            .WithOne()
            .HasForeignKey(h => new { h.OrganizationId, h.ResultId })
            .HasPrincipalKey(r => new { r.OrganizationId, r.Id })
            .OnDelete(DeleteBehavior.Restrict);
        builder
            .Navigation(x => x.StatusHistory)
            .HasField("_statusHistory")
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasIndex(x => x.NormalizedMessageId).IsUnique();
        builder.HasIndex(x => new
        {
            x.OrganizationId,
            x.Status,
            x.CreatedAt,
        });
        builder.HasIndex(x => new
        {
            x.OrganizationId,
            x.CorrelationStatus,
            x.CreatedAt,
        });
        builder.HasIndex(x => new { x.OrganizationId, x.OrderId });
        builder.HasIndex(x => new { x.OrganizationId, x.PlacerOrderId });
        builder.HasIndex(x => new { x.OrganizationId, x.AccessionNumber });
        builder.HasIndex(x => new { x.OrganizationId, x.SpecimenId });
        builder.HasIndex(x => x.CorrelationId);

        builder.IgnoreDomainEvents();
        builder.UseXminConcurrency();
        builder.UseSnakeCaseColumns();
    }
}
