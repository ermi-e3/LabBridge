using LabBridge.Domain.Features.OrderResultManagement.Entities;
using LabBridge.Infrastructure.Persistence.Configurations.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LabBridge.Infrastructure.Persistence.Configurations.OrderResultManagement;

public sealed class OrderLineConfiguration : IEntityTypeConfiguration<OrderLine>
{
    public void Configure(EntityTypeBuilder<OrderLine> builder)
    {
        builder.ToTable("lab_order_line");
        builder.HasDomainAssignedKey();
        builder.HasTenantAlternateKey();
        builder.Property(x => x.LineNumber).IsRequired();
        builder.Property(x => x.ExternalTestCode).HasMaxLength(100).IsRequired();
        builder.Property(x => x.ExternalCodeSystem).HasMaxLength(100);
        builder.Property(x => x.Description).HasMaxLength(500);
        builder.Property(x => x.CanonicalTestCode).HasMaxLength(100);
        builder.Property(x => x.InstrumentTestCode).HasMaxLength(100);

        builder.HasOrganizationOwner();
        builder.HasIndex(x => new { x.OrderId, x.LineNumber }).IsUnique();
        builder.UseSnakeCaseColumns();
    }
}
