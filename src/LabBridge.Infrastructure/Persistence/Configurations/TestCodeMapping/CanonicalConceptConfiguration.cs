using LabBridge.Domain.Features.InstrumentManagement.Enums;
using LabBridge.Domain.Features.TestCodeMapping.Entities;
using LabBridge.Infrastructure.Persistence.Configurations.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LabBridge.Infrastructure.Persistence.Configurations.TestCodeMapping;

public sealed class CanonicalConceptConfiguration : IEntityTypeConfiguration<CanonicalConcept>
{
    public void Configure(EntityTypeBuilder<CanonicalConcept> builder)
    {
        builder.ToTable("canonical_concepts", "testCode_mapping");
        builder.HasDomainAssignedKey();
        builder.HasTenantAlternateKey();
        builder.Property(x => x.Domain).HasEnumAsString(15);
        builder.Property(x => x.Code).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(1000);
        builder.Property(x => x.Status).HasEnumAsString(10).HasDefaultValue(ActiveStatus.Active);
        builder.Property(x => x.CreatedAt).IsRequired();
        builder.Property(x => x.UpdatedAt).IsRequired();

        builder.HasOrganizationOwner();
        builder
            .HasIndex(x => new
            {
                x.OrganizationId,
                x.Domain,
                x.Code,
            })
            .IsUnique();

        builder.IgnoreDomainEvents();
        builder.UseXminConcurrency();
        builder.UseSnakeCaseColumns();
    }
}
