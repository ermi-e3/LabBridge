using LabBridge.Domain.Features.InstrumentManagement.Entities;
using LabBridge.Domain.Features.InstrumentManagement.Enums;
using LabBridge.Infrastructure.Persistence.Configurations.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LabBridge.Infrastructure.Persistence.Configurations.InstrumentManagement;

public sealed class ExternalSystemConfiguration : IEntityTypeConfiguration<ExternalSystem>
{
    public void Configure(EntityTypeBuilder<ExternalSystem> builder)
    {
        builder.ToTable("external_systems", "instrument_management");
        builder.HasDomainAssignedKey();
        builder.HasTenantAlternateKey();
        builder.Property(x => x.Name).HasCaseInsensitiveText(200).IsRequired();
        builder.Property(x => x.SystemType).HasEnumAsString(10);
        builder.Property(x => x.Description).HasMaxLength(1000);
        builder.Property(x => x.IsSimulated).IsRequired();
        builder.Property(x => x.Status).HasEnumAsString(10).HasDefaultValue(ActiveStatus.Active);
        builder.Property(x => x.CreatedAt).IsRequired();
        builder.Property(x => x.UpdatedAt).IsRequired();

        builder.HasOrganizationOwner();
        builder.HasIndex(x => new { x.OrganizationId, x.Name }).IsUnique();

        builder.IgnoreDomainEvents();
        builder.UseXminConcurrency();
        builder.UseSnakeCaseColumns();
    }
}
