using LabBridge.Domain.Features.OrganizationManagement.Entities;
using LabBridge.Infrastructure.Persistence.Configurations.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LabBridge.Infrastructure.Persistence.Configurations.OrganizationManagement;

public sealed class OrganizationSettingConfiguration : IEntityTypeConfiguration<OrganizationSetting>
{
    public void Configure(EntityTypeBuilder<OrganizationSetting> builder)
    {
        //builder.ToTable("organization_settings");
        builder.ToTable("organization_settings", "organization_management", t => t.HasCheckConstraint("ck_organization_settings_version", "version >= 1"));
        builder.HasDomainAssignedKey();
        builder.Property(x => x.SettingDefinitionId).IsRequired();
        builder.Property(x => x.ValueJson).HasJsonbColumn().IsRequired();
        builder.Property(x => x.Version).IsRequired().HasDefaultValue(1);
        builder.Property(x => x.CreatedAt).IsRequired();
        builder.Property(x => x.UpdatedAt).IsRequired();

        builder.HasOrganizationOwner();
        builder
            .HasOne<SettingDefinition>()
            .WithMany()
            .HasForeignKey(x => x.SettingDefinitionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new { x.OrganizationId, x.SettingDefinitionId }).IsUnique();

        builder.IgnoreDomainEvents();
        builder.UseXminConcurrency();
        builder.UseSnakeCaseColumns();
    }
}
