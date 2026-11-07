using LabBridge.Domain.Features.InstrumentManagement.Entities;
using LabBridge.Domain.Features.MessageIngestion.Entities;
using LabBridge.Infrastructure.Persistence.Configurations.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LabBridge.Infrastructure.Persistence.Configurations.MessageIngestion;

/// <summary>Metadata-only, no payload. FK to the connector only; the original organization status is a snapshot string.</summary>
public sealed class InboundRefusalConfiguration : IEntityTypeConfiguration<InboundRefusal>
{
    public void Configure(EntityTypeBuilder<InboundRefusal> builder)
    {
        //builder.ToTable("inbound_refusals");
        builder.ToTable("inbound_refusals", "message_ingestion", t => t.HasCheckConstraint("ck_inbound_refusals_size", "size_bytes IS NULL OR size_bytes >= 0"));
        builder.HasDomainAssignedKey();
        builder.Property(x => x.Reason).HasEnumAsString(30);
        builder.Property(x => x.OrganizationStatus).HasMaxLength(20);
        builder.Property(x => x.ContentHash).HasMaxLength(64);
        builder.Property(x => x.TransportType).HasMaxLength(50);
        builder.Property(x => x.RemoteEndpoint).HasMaxLength(300);
        builder.Property(x => x.CorrelationId).IsRequired();
        builder.Property(x => x.TraceId).HasMaxLength(64);
        builder.Property(x => x.RefusedAt).IsRequired();

        builder.HasOrganizationOwner();
        builder
            .HasOne<Connector>()
            .WithMany()
            .HasForeignKey(x => new { x.OrganizationId, x.ConnectorId })
            .HasPrincipalKey(c => new { c.OrganizationId, c.Id })
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new { x.OrganizationId, x.RefusedAt }).IsDescending(false, true);
        builder.HasIndex(x => new { x.ConnectorId, x.RefusedAt });
        builder.UseSnakeCaseColumns();
    }
}
