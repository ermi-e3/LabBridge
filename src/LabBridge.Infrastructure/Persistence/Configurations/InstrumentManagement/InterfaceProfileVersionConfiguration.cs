using LabBridge.Domain.Features.InstrumentManagement.Entities;
using LabBridge.Infrastructure.Persistence.Configurations.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LabBridge.Infrastructure.Persistence.Configurations.InstrumentManagement;

/// <summary>Published definitions are immutable (enforced by migration policy and a database guard, FRS DB-CON-011).</summary>
public sealed class InterfaceProfileVersionConfiguration
    : IEntityTypeConfiguration<InterfaceProfileVersion>
{
    public void Configure(EntityTypeBuilder<InterfaceProfileVersion> builder)
    {
        builder.ToTable("interface_profile_versions", "instrument_management");
        builder.HasDomainAssignedKey();
        builder.Property(x => x.Version).HasMaxLength(30).IsRequired();
        builder.Property(x => x.Status).HasEnumAsString(12);
        builder.Property(x => x.DefinitionSchemaVersion).IsRequired();
        builder.Property(x => x.DefinitionJson).HasJsonbColumn().IsRequired();
        builder.Property(x => x.ReleaseNotes).HasMaxLength(2000);
        builder.Property(x => x.PublishedAt).IsRequired();

        builder.HasIndex(x => new { x.ProfileId, x.Version }).IsUnique();

        builder.UseSnakeCaseColumns();
    }
}
