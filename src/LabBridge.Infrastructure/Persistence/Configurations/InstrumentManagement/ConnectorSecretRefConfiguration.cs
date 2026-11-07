using LabBridge.Domain.Features.InstrumentManagement.Entities;
using LabBridge.Infrastructure.Persistence.Configurations.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LabBridge.Infrastructure.Persistence.Configurations.InstrumentManagement;

/// <summary>Holds references to the secret store only, never secret values (BR-CON-011). Removed only with a deleted Draft connector.</summary>
public sealed class ConnectorSecretRefConfiguration : IEntityTypeConfiguration<ConnectorSecretRef>
{
    public void Configure(EntityTypeBuilder<ConnectorSecretRef> builder)
    {
        builder.ToTable("connector_secret_refs");
        builder.HasDomainAssignedKey();
        builder.Property(x => x.Slot).HasMaxLength(50).IsRequired();
        builder.Property(x => x.SecretReference).HasMaxLength(300).IsRequired();
        builder.Property(x => x.Version).IsRequired();
        builder.Property(x => x.SetAt).IsRequired();

        builder.HasOrganizationOwner();
        builder.HasIndex(x => new { x.ConnectorId, x.Slot }).IsUnique();

        builder.UseSnakeCaseColumns();
    }
}
