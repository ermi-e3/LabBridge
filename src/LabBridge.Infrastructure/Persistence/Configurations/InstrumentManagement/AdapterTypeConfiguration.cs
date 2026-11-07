using LabBridge.Domain.Features.InstrumentManagement.Entities;
using LabBridge.Infrastructure.Persistence.Configurations.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LabBridge.Infrastructure.Persistence.Configurations.InstrumentManagement;

/// <summary>Platform reference data. Supported directions are persisted as text[] from the domain's private backing list.</summary>
public sealed class AdapterTypeConfiguration : IEntityTypeConfiguration<AdapterType>
{
    public void Configure(EntityTypeBuilder<AdapterType> builder)
    {
        //builder.ToTable("adapter_types");
        builder.ToTable("adapter_types", "instrument_management", t => t.HasCheckConstraint("ck_adapter_types_directions", "cardinality(supported_directions) >= 1"));
        builder.HasDomainAssignedKey();
        builder.Property(x => x.Code).HasMaxLength(100).IsRequired();
        builder.Property(x => x.DisplayName).HasMaxLength(200).IsRequired();
        builder.Property(x => x.SettingsSchemaJson).HasJsonbColumn().IsRequired();
        builder.Property(x => x.SecretSlotsJson).HasJsonbColumn().IsRequired();
        builder.Property(x => x.BindingAttributesJson).HasJsonbColumn();
        //builder.Property(x => x.BindingAttributesJson).HasColumnType("json");
        builder.Property(x => x.SupportsConnectionTest).IsRequired();
        builder.Property(x => x.IsSimulator).IsRequired();
        builder.Property(x => x.AdapterVersion).HasMaxLength(30).IsRequired();
        //builder.Property(x => x.IsActive).IsRequired().HasDefaultValue(true);
        builder.Property(x => x.IsActive).IsRequired();   // no HasDefaultValue(true): the domain always assigns it
        builder.Property(x => x.CreatedAt).IsRequired();

        builder.Ignore(x => x.SupportedDirections);
        builder
            .Property<List<string>>("_supportedDirectionNames")
            .HasColumnName("supported_directions")
            .HasColumnType("text[]")
            .IsRequired();

        builder.HasIndex(x => x.Code).IsUnique();

        builder.UseSnakeCaseColumns();
    }
}
