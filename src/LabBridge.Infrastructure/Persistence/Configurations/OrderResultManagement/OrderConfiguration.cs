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

/// <summary>
/// Physical table lab_order (avoids a reserved word). Contains protected patient data (Confidential).
/// ASSUMPTION A-WFL-01: the FRS leaves most varchar lengths open; identifiers 100, names 200, reasons 500, raw timestamps 100.
/// </summary>
public sealed class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        //builder.ToTable("lab_order");
        builder.ToTable(
                    "lab_order", "orderResult_management",
                    t =>
                    {
                        t.HasCheckConstraint(
                            "ck_lab_order_quarantine",
                            "status <> 'Quarantined' OR (quarantine_reason IS NOT NULL AND quarantined_from_status IS NOT NULL)"
                        );
                        t.HasCheckConstraint("ck_lab_order_content_hash", "content_hash ~ '^[0-9a-f]{64}$'");
                        t.HasCheckConstraint("ck_lab_order_payload_version", "payload_version >= 1");
                    }
                );
        builder.HasDomainAssignedKey();
        builder.HasTenantAlternateKey();
        builder.Property(x => x.CorrelationId).IsRequired();
        builder.Property(x => x.PlacerOrderId).HasMaxLength(100);
        builder.Property(x => x.FillerOrderId).HasMaxLength(100);
        builder.Property(x => x.AccessionNumber).HasMaxLength(100);
        builder.Property(x => x.SpecimenId).HasMaxLength(100);
        builder.Property(x => x.MessageControlId).HasMaxLength(100);
        builder.Property(x => x.PatientExternalId).HasMaxLength(100);
        builder.Property(x => x.PatientName).HasMaxLength(200);
        builder.Property(x => x.PatientDateOfBirth).HasColumnType("date");
        builder.Property(x => x.PatientSex).HasMaxLength(20);
        builder.Property(x => x.SpecimenType).HasMaxLength(100);
        builder.Property(x => x.SpecimenTypeCanonical).HasMaxLength(100);
        builder.Property(x => x.SpecimenTypeTarget).HasMaxLength(100);
        builder.Property(x => x.SpecimenCollectedAtRaw).HasMaxLength(100);
        builder.Property(x => x.Priority).HasMaxLength(50);
        builder.Property(x => x.PriorityCanonical).HasMaxLength(50);
        builder.Property(x => x.PriorityTarget).HasMaxLength(50);
        builder.Property(x => x.OrderedAtRaw).HasMaxLength(100);
        builder.Property(x => x.Status).HasEnumAsString(20).HasDefaultValue(OrderStatus.Received);
        builder.Property(x => x.StatusReason).HasMaxLength(500);
        builder.Property(x => x.QuarantineReason).HasEnumAsString(30);
        builder.Property(x => x.QuarantinedFromStatus).HasEnumAsString(20);
        builder.Property(x => x.ContentHash).HasMaxLength(64).IsRequired();
        builder.Property(x => x.PayloadVersion).IsRequired().HasDefaultValue(1);
        builder.Property(x => x.IsSimulated).IsRequired();
        builder.Property(x => x.CreatedAt).IsRequired();
        builder.Property(x => x.UpdatedAt).IsRequired();

        builder.HasOrganizationOwner();
        builder
            .HasOne<Connector>()
            .WithMany()
            .HasForeignKey(x => new { x.OrganizationId, x.SourceConnectorId })
            .HasPrincipalKey(c => new { c.OrganizationId, c.Id })
            .OnDelete(DeleteBehavior.Restrict);
        builder
            .HasOne<ExternalSystem>()
            .WithMany()
            .HasForeignKey(x => new { x.OrganizationId, x.SourceExternalSystemId })
            .HasPrincipalKey(s => new { s.OrganizationId, s.Id })
            .OnDelete(DeleteBehavior.Restrict);
        builder
            .HasOne<Route>()
            .WithMany()
            .HasForeignKey(x => new { x.OrganizationId, x.RouteId })
            .HasPrincipalKey(r => new { r.OrganizationId, r.Id })
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);
        builder
            .HasOne<Instrument>()
            .WithMany()
            .HasForeignKey(x => new { x.OrganizationId, x.TargetInstrumentId })
            .HasPrincipalKey(i => new { i.OrganizationId, i.Id })
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
            .HasOne<Order>()
            .WithMany()
            .HasForeignKey(x => new { x.OrganizationId, x.ConflictsWithOrderId })
            .HasPrincipalKey(o => new { o.OrganizationId, o.Id })
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasMany(x => x.Lines)
            .WithOne()
            .HasForeignKey(l => new { l.OrganizationId, l.OrderId })
            .HasPrincipalKey(o => new { o.OrganizationId, o.Id })
            .OnDelete(DeleteBehavior.Restrict);
        builder
            .Navigation(x => x.Lines)
            .HasField("_lines")
            .UsePropertyAccessMode(PropertyAccessMode.Field);
        builder
            .HasMany(x => x.StatusHistory)
            .WithOne()
            .HasForeignKey(h => new { h.OrganizationId, h.OrderId })
            .HasPrincipalKey(o => new { o.OrganizationId, o.Id })
            .OnDelete(DeleteBehavior.Restrict);
        builder
            .Navigation(x => x.StatusHistory)
            .HasField("_statusHistory")
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasIndex(x => x.NormalizedMessageId).IsUnique();
        builder
            .HasIndex(x => new
            {
                x.OrganizationId,
                x.SourceExternalSystemId,
                x.PlacerOrderId,
            })
            .IsUnique()
            .HasFilter("placer_order_id IS NOT NULL AND status NOT IN ('Quarantined','Rejected')")
            .HasDatabaseName("ux_lab_order_placer");
        builder.HasIndex(x => new
        {
            x.OrganizationId,
            x.Status,
            x.CreatedAt,
        });
        builder.HasIndex(x => new
        {
            x.OrganizationId,
            x.TargetInstrumentId,
            x.Status,
        });
        builder.HasIndex(x => new { x.OrganizationId, x.AccessionNumber });
        builder.HasIndex(x => new { x.OrganizationId, x.SpecimenId });
        builder.HasIndex(x => x.CorrelationId);

        builder.IgnoreDomainEvents();
        builder.UseXminConcurrency();
        builder.UseSnakeCaseColumns();
    }
}
