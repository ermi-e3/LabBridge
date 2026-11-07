using LabBridge.Domain.Features.OrderResultManagement.Entities;
using LabBridge.Infrastructure.Persistence.Configurations.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LabBridge.Infrastructure.Persistence.Configurations.OrderResultManagement;

public sealed class OrderStatusHistoryConfiguration : IEntityTypeConfiguration<OrderStatusHistory>
{
    public void Configure(EntityTypeBuilder<OrderStatusHistory> builder)
    {
        builder.ToTable("lab_order_status_history");
        builder.HasDomainAssignedKey();
        builder.Property(x => x.FromStatus).HasEnumAsString(20);
        builder.Property(x => x.ToStatus).HasEnumAsString(20);
        builder.Property(x => x.Reason).HasMaxLength(500);
        builder.Property(x => x.ActorType).HasEnumAsString(10);
        builder.Property(x => x.Source).HasEnumAsString(10);
        builder.Property(x => x.ChangedAt).IsRequired();

        builder.HasOrganizationOwner();
        builder.HasIndex(x => new { x.OrderId, x.ChangedAt });
        builder.UseSnakeCaseColumns();
    }
}
