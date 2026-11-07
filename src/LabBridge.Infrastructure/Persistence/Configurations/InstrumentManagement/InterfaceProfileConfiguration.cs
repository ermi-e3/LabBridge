using LabBridge.Domain.Features.InstrumentManagement.Entities;
using LabBridge.Infrastructure.Persistence.Configurations.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LabBridge.Infrastructure.Persistence.Configurations.InstrumentManagement;

/// <summary>Platform reference data (no tenant).</summary>
public sealed class InterfaceProfileConfiguration : IEntityTypeConfiguration<InterfaceProfile>
{
    public void Configure(EntityTypeBuilder<InterfaceProfile> builder)
    {
        builder.ToTable("interface_profiles", "instrument_management");
        builder.HasDomainAssignedKey();
        builder.Property(x => x.Code).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(1000);
        builder.Property(x => x.IsSimulation).IsRequired();
        builder.Property(x => x.CreatedAt).IsRequired();

        builder.HasIndex(x => x.Code).IsUnique();

        builder
            .HasMany(x => x.Versions)
            .WithOne()
            .HasForeignKey(v => v.ProfileId)
            .OnDelete(DeleteBehavior.Restrict);
        builder
            .Navigation(x => x.Versions)
            .HasField("_versions")
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.IgnoreDomainEvents();
        builder.UseSnakeCaseColumns();
    }
}
