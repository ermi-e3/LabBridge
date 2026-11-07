using LabBridge.Domain.Features.InstrumentManagement.Entities;
using LabBridge.Infrastructure.Persistence.Configurations.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LabBridge.Infrastructure.Persistence.Configurations.InstrumentManagement;

/// <summary>Append-only evidence (no update/delete through the application).</summary>
public sealed class InstrumentVerificationConfiguration
    : IEntityTypeConfiguration<InstrumentVerification>
{
    public void Configure(EntityTypeBuilder<InstrumentVerification> builder)
    {
        builder.ToTable("instrument_verifications");
        builder.HasDomainAssignedKey();
        builder.Property(x => x.Kind).HasEnumAsString(10);
        builder.Property(x => x.Outcome).HasEnumAsString(10);
        builder.Property(x => x.EvidenceReference).HasMaxLength(500).IsRequired();
        builder.Property(x => x.Notes).HasMaxLength(1000);
        builder.Property(x => x.PerformedAt).IsRequired();
        builder.Property(x => x.RecordedAt).IsRequired();

        builder.HasOrganizationOwner();
        builder
            .HasOne<InterfaceProfileVersion>()
            .WithMany()
            .HasForeignKey(x => x.ProfileVersionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasIndex(x => new
            {
                x.OrganizationId,
                x.InstrumentId,
                x.PerformedAt,
            })
            .IsDescending(false, false, true);

        builder.UseSnakeCaseColumns();
    }
}
