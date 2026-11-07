using LabBridge.Domain.Features.OrganizationManagement.Entities;
using LabBridge.Infrastructure.Persistence.Configurations.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LabBridge.Infrastructure.Persistence.Configurations.OrganizationManagement;

/// <summary>Platform reference data (no tenant). Changed by migration only.</summary>
public sealed class SettingDefinitionConfiguration : IEntityTypeConfiguration<SettingDefinition>
{
    public void Configure(EntityTypeBuilder<SettingDefinition> builder)
    {
        builder.ToTable("setting_definitions", "organization_management");
        builder.HasDomainAssignedKey();
        builder.Property(x => x.Key).HasMaxLength(100).IsRequired();
        builder.Property(x => x.ValueType).HasEnumAsString(20);
        builder.Property(x => x.ConstraintsJson).HasJsonbColumn();
        builder.Property(x => x.DefaultValueJson).HasJsonbColumn();
        builder.Property(x => x.PlatformMinJson).HasJsonbColumn();
        builder.Property(x => x.PlatformMaxJson).HasJsonbColumn();
        builder.Property(x => x.EditableBy).HasEnumAsString(20);
        builder.Property(x => x.OwnerModule).HasMaxLength(3).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(500).IsRequired();
        //builder.Property(x => x.IsActive).IsRequired().HasDefaultValue(true);
        builder.Property(x => x.IsActive).IsRequired();   // no HasDefaultValue(true): Define always assigns it
        builder.Property(x => x.CreatedAt).IsRequired();

        builder.HasIndex(x => x.Key).IsUnique();

        builder.UseSnakeCaseColumns();
    }
}
